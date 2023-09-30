﻿using DevExpress.ExpressApp;
using OutlookInspired.Win.Editors;
using OutlookInspired.Win.Extensions.Internal;

namespace OutlookInspired.Win.Features.Quotes{
    public class PropertyEditorController:ViewController<ListView>{
        protected override void OnViewControlsCreated(){
            base.OnViewControlsCreated();
            if (View.Editor is not DevExpress.ExpressApp.PivotGrid.Win.PivotGridListEditor pivotGridListEditor) return;
            var pivotGridControl = pivotGridListEditor.PivotGridControl;
            var repositoryItems = pivotGridControl.AddRepositoryItems(View);
            pivotGridControl.CustomCellEdit += (_, e) => {
                if (repositoryItems.TryGetValue(e.DataField, out var item)){
                    e.RepositoryItem = item;
                }
            };
            pivotGridControl.CustomCellValue += (_, e) => {
                if (repositoryItems.TryGetValue(e.DataField, out var item)&&item is IValueCalculator valueCalculator){
                    e.Value = valueCalculator.Calculate(e.Value);  
                }
            };
            pivotGridControl.CustomDrawCell += (_, e) => {
                if (repositoryItems.TryGetValue(e.DataField, out var item)){
                    e.Appearance = item.Appearance;
                }
            };
        }
    }
}