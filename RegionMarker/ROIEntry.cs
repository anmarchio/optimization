using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HalconDotNet;

namespace PRIME.RegionMarker
{

    public partial class ROIEntry : UserControl
    {
        public ROIEntry(int index, string type, HObject region)
        {
            InitializeComponent();
            ROItype = type;
            Region = region;
            Index = index;

            HTuple area, column, row;
            HOperatorSet.AreaCenter(region, out area, out row, out column);
            CenterColumn = (int)column.D;
            CenterRow = (int)row.D;
        }
        private int index;
        public int Index
        {
            get
            {
                return index;
            }
            set
            {
                index = value;
                Text = value + "_" + ROIType;
            }
        }
        private string ROItype;

        public new HObject Region { get; private set; }
        public string ROIType
        {
            get
            {
                return ROItype;
            }
            set
            {
                ROItype = value;
                Text = Index + "_" + value;
            }
        }

        public int CenterRow
        {
            get; private set;
        }
        public int CenterColumn
        {
            get; private set;
        }
    }
}
