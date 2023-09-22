﻿using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Win.Editors;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using OutlookInspired.Module.Attributes;
using EditorAliases = OutlookInspired.Module.Services.Internal.EditorAliases;

namespace OutlookInspired.Win.Editors{
    [PropertyEditor(typeof(object),EditorAliases.LabelPropertyEditor,false)]
    public class LabelControlPropertyEditor : WinPropertyEditor{
        public LabelControlPropertyEditor(Type objectType, IModelMemberViewItem model) : base(objectType, model) 
            => ControlBindingProperty = "Text";

        public override bool CanFormatPropertyValue => true;

        protected override object CreateControlCore() 
            => new LabelControl{
                BorderStyle = BorderStyles.NoBorder,
                AutoSizeMode = LabelAutoSizeMode.None,
                ShowLineShadow = false,
                Appearance ={
                    FontSizeDelta = MemberInfo.FindAttribute<FontSizeDeltaAttribute>()?.Delta??0,
                    TextOptions = { WordWrap =MemberInfo.Size==-1? WordWrap.Wrap:WordWrap.Default}
                }
            };

        protected override void ReadValueCore(){
            base.ReadValueCore();
            Control.Text =DisplayFormat!=String.Empty? string.Format(DisplayFormat,PropertyValue):$"{PropertyValue}";
        }
    }
}