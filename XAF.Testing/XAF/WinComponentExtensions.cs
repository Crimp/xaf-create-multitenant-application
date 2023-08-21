﻿using System.Collections;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using DevExpress.Data.Linq;
using DevExpress.ExpressApp;
using DevExpress.Utils.Controls;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Base.Handler;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.Handler;
using DevExpress.XtraGrid.Views.Layout;
using DevExpress.XtraGrid.Views.Layout.Handler;
using DevExpress.XtraScheduler;
using DevExpress.XtraScheduler.Xml;
using XAF.Testing.RX;

namespace XAF.Testing.XAF{
    public static class WinComponentExtensions{
        public static int SelectRow<T>(this GridView gridView, T row) where T : class{
            var rowHandle = gridView.FindRow(row);
            gridView.MakeRowVisible(rowHandle);
            gridView.SelectRow(rowHandle);
            return rowHandle;
        }
        public static object FocusedRowObjectKey(this ColumnView columnView, IObjectSpace objectSpace) 
            => columnView.IsServerMode ? columnView.FocusedRowObject : objectSpace.GetKeyValue(columnView.FocusedRowObject);
        public static object FocusRowObject(this ColumnView columnView, IObjectSpace objectSpace,Type objectType) 
            => columnView.FocusedRowObject == null || !columnView.IsServerMode ? columnView.FocusedRowObject
                : objectSpace.GetObjectByKey(objectType, columnView.FocusedRowObject);

        public static void AddNewRow(this GridView gridView,params (string fieldName,object value)[] values){
            gridView.AddNewRow();
            gridView.FocusedRowHandle = GridControl.NewItemRowHandle;
            values.Do(t => gridView.SetRowCellValue(gridView.FocusedRowHandle, t.fieldName, t.value)).Enumerate();
            gridView.UpdateCurrentRow();
        } 
        public static string YearlyOnWorkDayRecurrenceInfoXml(this DateTime now) 
            => new RecurrenceInfoXmlPersistenceHelper(new RecurrenceInfo(now){
                Type = RecurrenceType.Yearly, Periodicity = 1, Month = now.Month, WeekOfMonth = WeekOfMonth.First,
                Duration = TimeSpan.FromHours(1), WeekDays = WeekDays.WorkDays
            }).ToXml();


        public static IObservable<object> WhenDataSourceChanged(this GridControl gridControl) 
            => gridControl.WhenEvent(nameof(GridControl.DataSourceChanged));

        public static IObservable<object> GridDetailViewObjects(this GridView view) 
            => view.WhenEvent<CustomMasterRowEventArgs>(nameof(GridView.MasterRowExpanded))
                .Select(e => view.GetDetailView(e.RowHandle,e.RelationIndex)).Cast<ColumnView>()
                .Delay(100.Milliseconds(),new SynchronizationContextScheduler(SynchronizationContext.Current!))
                .SelectMany(baseView => ((IEnumerable)baseView.DataSource).Cast<object>().Take(1).ToArray())
                .Take(view.GridControl.LevelTree.Nodes.Count).BufferUntilCompleted().SelectMany()
                .MergeToObject(view.Observe().Do(gridView => gridView.RecursiveExpandAndFocus(0)).IgnoreElements());

        public static void RecursiveExpandAndFocus(this GridView masterView, int masterRowHandle){
            var relationCount = masterView.GetRelationCount(masterRowHandle);
            for (var index = 0; index < relationCount; index++){
                masterView.ExpandMasterRow(masterRowHandle, index);
                if (masterView.GetDetailView(masterRowHandle, index) is GridView childView){
                    childView.FocusedRowHandle = 0;
                    for (var handle = 0; handle < childView.DataRowCount; handle++)
                        RecursiveExpandAndFocus(childView, handle);
                }
            }
        }
        public static IObservable<ColumnView> ProcessEvent(this IObservable<GridControl> source,EventType eventType)
            => source.SelectMany(control => control.ProcessEvent(eventType));

        public static IObservable<ColumnView> ProcessEvent(this GridControl control,EventType eventType) 
            => ((ColumnView)control.MainView).ProcessEvent(eventType);

        public static IObservable<ColumnView> ProcessEvent(this ColumnView columnView,EventType eventType) 
            => columnView.WhenEvent<FocusedRowObjectChangedEventArgs>(nameof(columnView.FocusedRowObjectChanged))
                .WhenNotDefault(e => e.Row).StartWith(columnView.FocusedRowObject).WhenNotDefault()
                .Do(_ => columnView.ViewHandler().ProcessEvent(eventType, EventArgs.Empty))
                .To(columnView)
                .Merge(Observable.Defer(() => {
                    var row = columnView.FindRow(columnView.YieldDataSource().First());
                    columnView.Focus();
                    columnView.SelectRow(row);
                    columnView.SelectRow(2);
                    columnView.FocusedRowHandle = 2;
                    return Observable.Empty<ColumnView>();
                }).To<ColumnView>());

        private static BaseViewHandler ViewHandler(this ColumnView columnView) 
            => columnView is LayoutView layoutView ? new LayoutViewHandler(layoutView) : new GridHandler((GridView)columnView);

        public static IObservable<object> HasObjects(this IObservable<GridControl> source)
            => source.SelectMany(control => control.MainView.YieldDataSource());

        private static IEnumerable<object> YieldDataSource(this BaseView control) 
            => control.DataSource == null ? Enumerable.Empty<object>() : control.DataSource is EntityServerModeSource source
                ? source.QueryableSource.Cast<object>() : control.DataSource is EntityServerModeFrontEnd frontEndSource
                    ? 0.Range(frontEndSource.Count).Select(i => frontEndSource[i]).Where(row => row != null)
                    : ((IEnumerable)control.DataSource).Cast<object>();
    }
}