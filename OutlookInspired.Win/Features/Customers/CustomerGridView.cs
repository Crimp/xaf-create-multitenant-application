﻿using OutlookInspired.Module.BusinessObjects;
using OutlookInspired.Win.Editors;

namespace OutlookInspired.Win.Features.Customers
{
    public partial class CustomerGridView : ColumnViewUserControl
    {
        public CustomerGridView()
        {
            InitializeComponent();
        }

        public override Type ObjectType => typeof(Customer);
    }
}