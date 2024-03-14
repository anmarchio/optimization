using System;
using System.Windows.Forms;
using System.IO;
using HalconDotNet;
using System.Reflection;
using System.Threading;
using System.Linq;
using Extensions;
using Optimization.HPipeline;
using Optimization.Serialization;

namespace PRIME.RegionMarker
{
    public partial class RegionMarker : MetroFramework.Forms.MetroForm//Form
    {

        private HImage image;
        // variables for scrolling (current top left img position)
        private int imgX = 0, imgY = 0;
        private HTuple imgWidth, imgHeight;
        string imgFilename;
        private int windowRow, windowColumn, windowWidth, windowHeight;
        private string[] ROITypes;
        private bool fill = false;
        private bool drawing = false;
        private bool hideLabels = false;
        private static int KEYSCROLLING = 120;
        

        private string[] colors = new string[] { "white", "red", "green", "blue", "cyan", "magenta", "yellow", "coral", "slate blue", "spring green", "orange red", "orange", "dark olive green", "pink"
        };

        private string selectedColor = "red", regionColor = "blue", textColor = "white";

        private const int NO_REGION_SELECTED = -1;
        private const int NEW_REGION_SELECTED = -2;

        public RegionMarker(string[] ROITypes)
        {
            InitializeComponent();
            hWindowControl1.HalconWindow.GetWindowExtents(out windowRow, out windowColumn, out windowWidth, out windowHeight);
            this.ROITypes = ROITypes;
            listROIs.MouseDown += listROIs_MouseDown;
            listROIs.SelectionMode = SelectionMode.MultiExtended;
            listROIs.DisplayMember = "Text";

            foreach (var s in ROITypes)
            {
                contextROI.Items.Add(s);
            }

            contextROI.Items.Add(new ToolStripSeparator());

            contextROI.Items.Add("Union");
            contextROI.Items.Add("Difference");
            contextROI.Items.Add("Intersection");
            contextROI.Items.Add("Split");
            contextROI.Items.Add("Delete");
            contextROI.Items.Add("Write Histogram to .csv");

            hWindowControl1.HalconWindow.SetColor(regionColor);
            hWindowControl1.HalconWindow.SetDraw("margin");
            hWindowControl1.HalconWindow.SetPartStyle(0);
            listROIs.MouseDown += ListROISs_MouseClick;
            listROIs.MouseDown += listROIs_MouseDown;
            listROIs.DoubleClick += ListROIs_DoubleClick;
            contextROI.ItemClicked += contextROI_ItemClick;
            KeyPreview = true;
            KeyDown += new KeyEventHandler(KeyPress);
            MouseWheel += mouseWheel;

            toolStripMenuItem4.Checked = true;  // keyscrolling == 120

            //listROIs.ContextMenuStrip = contextROI;
            //hWindowControl1.MouseClick += newRegionToolStripMenuItem_Click;
            //TestListBox();
            InitializeColorSelection();
            InitializePipelineSelection();
        }

        private void InitializeColorSelection()
        {
            for (int i = 0; i < colors.Length; i++)
            {
                regionColorToolStripMenuItem.DropDownItems.Add(colors[i]);
                regionColorToolStripMenuItem.DropDownItems[i].Click += regionColorClick;
                selectedColorToolStripMenuItem.DropDownItems.Add(colors[i]);
                selectedColorToolStripMenuItem.DropDownItems[i].Click += selectedColorClick;
                textColorToolStripMenuItem.DropDownItems.Add(colors[i]);
                textColorToolStripMenuItem.DropDownItems[i].Click += textColorClick;

            }

            var item = regionColorToolStripMenuItem.DropDownItems[3] as ToolStripMenuItem;
            item.Checked = true;
            item = selectedColorToolStripMenuItem.DropDownItems[1] as ToolStripMenuItem;
            item.Checked = true;
            item = textColorToolStripMenuItem.DropDownItems[0] as ToolStripMenuItem;
            item.Checked = true;
        }

        private void InitializePipelineSelection()
        {
            foreach(var pipeline in CommonHalconPipelines.Collection)
            {
                var item = new ToolStripMenuItem(pipeline.Name);
                item.Click += segmentUsingPipeline;
                commonPipelinesToolStripMenuItem.DropDownItems.Add(item);
            }
        }

        private void segmentUsingPipeline(object sender, EventArgs e)
        {
            var s = sender as ToolStripMenuItem;
            var pipeName = s.Text;
            var pipe = CommonHalconPipelines.HalconPipelineDictionary[pipeName];
            SegmentUsingPipeline(pipe);
        }

        private void regionColorClick(object sender, EventArgs e)
        {
            foreach (ToolStripMenuItem i in regionColorToolStripMenuItem.DropDownItems)
            {
                i.Checked = false;
            }
            var item = sender as ToolStripMenuItem;
            regionColor = item.Text;
            item.Checked = true;

            DisplayImage();
        }

        private void selectedColorClick(object sender, EventArgs e)
        {
            foreach (ToolStripMenuItem i in selectedColorToolStripMenuItem.DropDownItems)
            {
                i.Checked = false;
            }
            var item = sender as ToolStripMenuItem;
            selectedColor = item.Text;
            item.Checked = true;

            DisplayImage();

        }

        private void textColorClick(object sender, EventArgs e)
        {
            foreach (ToolStripMenuItem i in textColorToolStripMenuItem.DropDownItems)
            {
                i.Checked = false;
            }
            var item = sender as ToolStripMenuItem;
            textColor = item.Text;
            item.Checked = true;

            DisplayImage();

        }

        private void mouseWheel(object sender, MouseEventArgs e)
        {
            var delta = e.Delta;

            if (delta < 0)
            {
                windowHeight *= 2;
                windowWidth *= 2;
            }
            else
            {
                windowHeight /= 2;
                windowWidth /= 2;
            }


            /*
            var imgY = this.imgY - delta;

            this.imgY = imgY > 0 ? imgY : 0;
            */

            DisplayImage();
            UpdateScrollbars();
        }

        private void TestListBox()
        {
            for (int i = 0; i < 500; i++)
            {
                listROIs.Items.Add(new ROIEntry(i, i.ToString(), null));
            }
        }

        private new void KeyPress(object sender, KeyEventArgs e)
        {
            var keyCode = e.KeyCode;
            if (keyCode == Keys.Space)
            {
                newRegionToolStripMenuItem_Click(sender, e);
            }
            else if (keyCode == Keys.Delete)
            {
                DeleteListEntries();
            }
            else if (keyCode == Keys.F)
            {
                fill = !fill;
                var s = fillOnOffToolStripMenuItem;
                s.Checked = !s.Checked;
            }else if(keyCode == Keys.Escape)
            {
                if(listROIs.Focused)
                {
                    listROIs.SelectedItems.Clear();
                }
            }

            if (keyCode == Keys.W ||keyCode == Keys.Up)
            {
                var imgY = this.imgY - KEYSCROLLING;
                this.imgY = imgY > 0 ? imgY : 0;
            }
            else if (keyCode == Keys.S || keyCode == Keys.Down)
            {
                var imgY = this.imgY + KEYSCROLLING;
                this.imgY = imgY > 0 ? imgY : 0;
            }

            if (keyCode == Keys.A || keyCode == Keys.Left)
            {
                var imgX = this.imgX - KEYSCROLLING;
                this.imgX = imgX > 0 ? imgX : 0;
            }
            else if (keyCode == Keys.D || keyCode == Keys.Right)
            {
                var imgX = this.imgX + KEYSCROLLING;
                this.imgX = imgX > 0 ? imgX : 0;
            }


            UpdateScrollbars();
            DisplayImage();

        }

        private void UpdateScrollbars()
        {
            if (image == null) return;
            var nVal = (int)(imgX / (double)imgWidth.I * 100.0);
            hScrollBar1.Value = nVal < 101 ? nVal : 100;
            nVal = (int)(imgY / (double)imgHeight.I * 100.0);
            vScrollBar1.Value = nVal < 101 ? nVal : 100;
        }

        private void listROIs_MouseDown(object sender, MouseEventArgs e)
        {
            /*
            var idx = listROIs.IndexFromPoint(e.X, e.Y);
            if (listROIs.SelectedIndices.Contains(idx)) listROIs.SelectedIndices.Remove(listROIs.SelectedItems.IndexOf(idx));
            else
                listROIs.SelectedIndices.Add(listROIs.IndexFromPoint(e.X, e.Y));*/
        }

        public void BuildImageAndRegionsFilePaths(string fullPathToImage, out string fullPathToImagesFolder, out string fullPathToRegionsFolder)
        {
            var imageName = Path.GetFileName(fullPathToImage);
            var parent = Directory.GetParent(fullPathToImage);
            if (!parent.Name.Equals("images"))
            {
                fullPathToRegionsFolder = Path.Combine(parent.FullName, "regions");
                fullPathToImagesFolder = Path.Combine(parent.FullName, "images");
                if (!Directory.Exists(fullPathToRegionsFolder)) Directory.CreateDirectory(fullPathToRegionsFolder);
                if (!Directory.Exists(fullPathToImagesFolder)) Directory.CreateDirectory(fullPathToImagesFolder);
            }
            else
            {
                var parentParent = parent.Parent.FullName;
                fullPathToRegionsFolder = Path.Combine(parentParent, "regions");
                if (!Directory.Exists(fullPathToRegionsFolder)) Directory.CreateDirectory(fullPathToRegionsFolder);
                fullPathToImagesFolder = parent.FullName;
            }
        }

        private void loadImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fileDialog = new OpenFileDialog();

            try
            {
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    image = new HImage(fileDialog.FileName);
                    imgFilename = fileDialog.FileName;
                    currentImageLabel.Text = Path.GetFileName(imgFilename);

                    if (image != null)
                    {
                        imgX = 0; imgY = 0;
                        HOperatorSet.GetImageSize(image, out imgWidth, out imgHeight);
                        listROIs.Items.Clear();

                        if (MessageBox.Show("Autoimport regions?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            var oldPathStyle = Path.Combine(imgFilename.Replace(currentImageLabel.Text, ""), "regions", currentImageLabel.Text); // used to write reagions in folders ending with .jpg for lack of better judgement
                            string parentDir = null;
                            if (Directory.GetParent(imgFilename).Name.Equals("images"))
                                parentDir = Directory.GetParent(imgFilename).Parent.FullName;
                            else
                                parentDir = Directory.GetParent(imgFilename).FullName;
                            var newPathStyle = Path.Combine(parentDir, "regions", Path.GetFileNameWithoutExtension(currentImageLabel.Text));
                            string[] filenames = null;
                            if (Directory.Exists(oldPathStyle)) // for backwards compatibility
                            {
                                filenames = Directory.EnumerateFiles(oldPathStyle).ToArray();
                            }
                            else if (Directory.Exists(newPathStyle))  // format used to write newer regions
                            {
                                filenames = Directory.EnumerateFiles(newPathStyle).ToArray();
                            }
                            else if (Directory.Exists(newPathStyle + ".bmp"))
                            {
                                filenames = Directory.EnumerateFiles(newPathStyle + ".bmp").ToArray();
                            }
                            if (filenames == null)
                            {
                                MessageBox.Show("Folder Structure invalid");
                            }
                            else
                                LoadRegions(filenames);
                        }                   
                        UpdateIndices();
                        DisplayImage();

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace, ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void listROIs_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayImage();
        }
        
        private void ListROISs_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var idx = listROIs.IndexFromPoint(e.X, e.Y);
                listROIs.SelectedIndices.Add(listROIs.IndexFromPoint(e.X, e.Y));
                var l = sender as ListBox;
                contextROI.Show(Cursor.Position);
            }
        }
        
        private void ListROIs_DoubleClick(object sender, EventArgs e)
        {
            var mouse = e as MouseEventArgs;
            var idx = listROIs.IndexFromPoint(mouse.X, mouse.Y);
            if (idx == -1) return;
            var entry = (ROIEntry)listROIs.Items[idx];
            HTuple row, column, area;
            HOperatorSet.AreaCenter(entry.Region, out area, out row, out column);
            var nVal = row.D - windowHeight / 2;
            nVal = nVal > 0 ? nVal : 0;
            imgY = (int)nVal;
            nVal = nVal = column.D - windowHeight / 2;
            nVal = nVal > 0 ? nVal : 0;
            imgX = (int)nVal;

            UpdateScrollbars();
            DisplayImage();
        }

        private void DeleteListEntries()
        {
            var selectedIndices = listROIs.SelectedIndices;
            var count = selectedIndices.Count;
            for (int i = count - 1; i > -1; i--)
            {
                listROIs.Items.RemoveAt(selectedIndices[i]);
            }
        }
        
        private void contextROI_ItemClick(object sender, ToolStripItemClickedEventArgs e)
        {

            if ((e.ClickedItem.Text.Equals("Delete")))
            {
                DeleteListEntries();
            }
            else if(e.ClickedItem.Text.Equals("Write Histogram to .csv"))
            {
                HTuple absHisto, relHisto;
                var selectedIndices = listROIs.SelectedIndices;
                if (selectedIndices.Count <= 0) return;
                var label = ((ROIEntry)listROIs.Items[selectedIndices[0]]).ROIType;
                var regions = ((ROIEntry)listROIs.Items[selectedIndices[0]]).Region;
                for (int i = 1; i < selectedIndices.Count; i++)
                    regions = regions.ConcatObj(((ROIEntry)listROIs.Items[selectedIndices[0]]).Region[i]);
                HOperatorSet.GrayHisto(regions, image, out absHisto, out relHisto);
                /*var histView = new HistogramView(relHisto);
                histView.ShowDialog();*/
                var arr = relHisto.DArr;
                CSVWriter.WriteWithRowLabels(arr.ToRowVector(), new string[] { currentImageLabel.Text + "_" +   label}, "histogram.csv", true);
            }
            else if (e.ClickedItem.Text.Equals("Union"))
            {
                int[] selectedIndices = new int[listROIs.SelectedIndices.Count];
                listROIs.SelectedIndices.CopyTo(selectedIndices, 0);
                if (selectedIndices.Length < 2) return;
                var newRegion = ((ROIEntry)listROIs.Items[selectedIndices[selectedIndices.Length - 1]]).Region;
                var count = selectedIndices.Length;
                var newIndex = selectedIndices[0];
                for (int i = count - 2; i > -1; i--)
                {
                    HOperatorSet.Union2(newRegion, ((ROIEntry)listROIs.Items[selectedIndices[i]]).Region, out newRegion);
                    //newRegion.ConcatObj(((ROIEntry)listROIs.Items[selectedIndices[i]]).Region);                  
                }
                for (int i = count - 1; i > -1; i--)
                {
                    listROIs.Items.RemoveAt(selectedIndices[i]);
                }
                HOperatorSet.Union1(newRegion, out newRegion);
                var newEntry = new ROIEntry(newIndex, "", newRegion);
                listROIs.Items.Insert(newIndex, newEntry);
            }
            else if (e.ClickedItem.Text.Equals("Difference"))
            {
                var selectedIndices = listROIs.SelectedIndices;
                if (selectedIndices.Count != 2) return;
                var greater = ((ROIEntry)listROIs.Items[selectedIndices[1]]).Region;
                var smaller = ((ROIEntry)listROIs.Items[selectedIndices[0]]).Region;
                var newIndex = selectedIndices[0];
                HTuple row1, row2, column1, column2, area1, area2;

                HOperatorSet.AreaCenter(greater, out area1, out row1, out column1);
                HOperatorSet.AreaCenter(smaller, out area2, out row2, out column2);
                HObject newRegion;
                if (area1 > area2)
                {
                    HOperatorSet.Difference(greater, smaller, out newRegion);
                }
                else
                {
                    HOperatorSet.Difference(smaller, greater, out newRegion);
                }

                listROIs.Items.RemoveAt(selectedIndices[1]);
                listROIs.Items.RemoveAt(selectedIndices[0]);

                var newEntry = new ROIEntry(newIndex, "", newRegion);
                listROIs.Items.Insert(newIndex, newEntry);
            }
            else if (e.ClickedItem.Text.Equals("Split"))
            {
                var selectedIndices = listROIs.SelectedIndices;
                if (selectedIndices.Count != 1) return;
                var entry = (ROIEntry)listROIs.Items[selectedIndices[0]];
                HObject newRegions;


                HOperatorSet.Connection(entry.Region, out newRegions);

                int idx = selectedIndices[0];
                listROIs.Items.RemoveAt(selectedIndices[0]);

                for (int i = 0; i < newRegions.CountObj(); i++)
                {
                    listROIs.Items.Insert(idx + i, new ROIEntry(idx, "", newRegions.SelectObj(i + 1)));
                }
            }
            else if(e.ClickedItem.Text.Equals("Intersection"))
            {
                var selectedIndices = listROIs.SelectedIndices;
                if (selectedIndices.Count != 2) return;
                var greater = ((ROIEntry)listROIs.Items[selectedIndices[1]]).Region;
                var smaller = ((ROIEntry)listROIs.Items[selectedIndices[0]]).Region;
                var newIndex = selectedIndices[0];
                HTuple row1, row2, column1, column2, area1, area2;

                HOperatorSet.AreaCenter(greater, out area1, out row1, out column1);
                HOperatorSet.AreaCenter(smaller, out area2, out row2, out column2);
                HObject newRegion;
                
                HOperatorSet.Intersection(smaller, greater, out newRegion);
                

                listROIs.Items.RemoveAt(selectedIndices[1]);
                listROIs.Items.RemoveAt(selectedIndices[0]);

                var newEntry = new ROIEntry(newIndex, "", newRegion);
                listROIs.Items.Insert(newIndex, newEntry);
            }
            else if(e.ClickedItem.Text.Equals("FillUp"))
            {

            }
            else
            {
                foreach (var type in ROITypes)
                {
                    if (type.Equals(e.ClickedItem.Text))
                    {
                        var items = listROIs.SelectedItems;
                        foreach (var item in items)
                        {
                            var i = item as ROIEntry;
                            i.ROIType = e.ClickedItem.Text;
                        }

                        break;
                    }
                }
            }

            UpdateIndices();
            DisplayImage();
        }

        private void DisplayImage()
        {
            if (image == null) return;
            hWindowControl1.HalconWindow.SetPart(imgY, imgX, (imgY + windowHeight), (imgX + windowWidth));
            hWindowControl1.HalconWindow.ClearWindow();
            hWindowControl1.HalconWindow.DispObj(image);

            var selectedIndices = listROIs.SelectedIndices;
            var count = listROIs.Items.Count;

            if (fill) hWindowControl1.HalconWindow.SetDraw("fill");
            else hWindowControl1.HalconWindow.SetDraw("margin");

            for (int i = 0; i < count; i++)
            {
                var r = (ROIEntry)listROIs.Items[i];
                if (selectedIndices.Contains(i)) hWindowControl1.HalconWindow.SetColor(selectedColor);
                hWindowControl1.HalconWindow.DispObj(r.Region);
                hWindowControl1.HalconWindow.SetTposition(r.CenterRow, r.CenterColumn);
                hWindowControl1.HalconWindow.SetColor(textColor);
                if (!hideLabels)
                {
                    hWindowControl1.HalconWindow.WriteString(r.Text);
                }
                hWindowControl1.HalconWindow.SetColor(regionColor);
            }
        }

        private void fillOnOffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fill = !fill;
            var s = sender as ToolStripMenuItem;
            s.Checked = !s.Checked;
            DisplayImage();
        }

        private void hideLabelsHToolStripMenuItem_Click(object sender, EventArgs e)
        {
            hideLabels = !hideLabels;
            var s = sender as ToolStripMenuItem;
            s.Checked = !s.Checked;
            DisplayImage();
        }

        private void keyScrollingToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            UncheckKeyScrolling();
            KEYSCROLLING = 20;
            var s = sender as ToolStripMenuItem;
            s.Checked = true;
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            UncheckKeyScrolling();
            KEYSCROLLING = 60;
            var s = sender as ToolStripMenuItem;
            s.Checked = true;

        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            UncheckKeyScrolling();
            KEYSCROLLING = 120;
            var s = sender as ToolStripMenuItem;
            s.Checked = true;
        }


        private void UncheckKeyScrolling()
        {
            foreach (ToolStripMenuItem k in keyScrollingToolStripMenuItem.DropDownItems)
            {
                k.Checked = false;
            }
        }

        private void saveRegionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (image == null || string.IsNullOrEmpty(this.imgFilename)) return;
            var date = DateTime.Now;
            var imageName = Path.GetFileName(this.imgFilename);

            string regionsDir = null, imagesDir = null;
            BuildImageAndRegionsFilePaths(imgFilename, out imagesDir, out regionsDir);

            var extension = Path.GetExtension(imgFilename);
            var fullNewImagePath = Path.Combine(imagesDir, imageName);
            if (!File.Exists(fullNewImagePath))
                HOperatorSet.WriteImage(image, extension.Replace(".", ""), 0, fullNewImagePath);

            var currentImageRegionDir = Path.Combine(regionsDir, Path.GetFileNameWithoutExtension(imageName));
            if (!Directory.Exists(currentImageRegionDir)) Directory.CreateDirectory(currentImageRegionDir);

            var files = Directory.GetFiles(currentImageRegionDir);
            if (files.Length > 0)
            {
                var result = MessageBox.Show("You are about to override all previously saved regions.", "Override Warning", MessageBoxButtons.OKCancel);
                if (result == DialogResult.Cancel) return;
            }
            foreach(var f in files)
            {
                File.Delete(f);
            }

            if (!AllEntriesLabelled())
            {
                MessageBox.Show("Not all ROIs are labelled.", "Labelling Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            for (int i = 0; i < listROIs.Items.Count; i++)
            {
                var entry = (ROIEntry)listROIs.Items[i];
                HOperatorSet.WriteRegion(entry.Region, Path.Combine(currentImageRegionDir, entry.ROIType + "_" + entry.Index + ".hobj"));
            }

            MessageBox.Show("ROIs saved successfully.", "Glorious Saving Success", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void loadRegionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            try
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    LoadRegions(dialog.FileNames);
                }
                UpdateIndices();
                DisplayImage();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace, ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SegmentUsingPipeline(HalconPipeline pipeline)
        {
            if (image == null) return;
            try
            {
                var result = pipeline.ExecuteSingle(image);
                var idx = listROIs.Items.Count;
                for (int i = 1; i <= result.CountObj(); i++)
                    listROIs.Items.Add(new ROIEntry(idx + i - 1, "", result.SelectObj(i)));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            DisplayImage();
        }

        private void loadPipelineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            if(dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var filename = dialog.FileName;
                    if (string.IsNullOrEmpty(filename)) return;
                    HalconPipeline pipeline = null;
                    try
                    {
                        pipeline = HalconPipeline.DeserializeBinary(filename) as HalconPipeline;
                    }
                    catch (Exception)
                    {
                        // ignored, just to try and error for binary or xml format
                    }
                    try
                    {
                        pipeline = HalconPipeline.DeserializeXml(filename) as HalconPipeline;
                    }
                    catch(Exception)
                    {
                        // ignored, just to try and error for binary or xml format
                    }
                    if (pipeline == null) return;
                    var item = new ToolStripMenuItem(filename, null, (s, ex) => { SegmentUsingPipeline(pipeline); });
                    usingPipelineToolStripMenuItem.DropDownItems.Add(item);
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Something went wrong loading the pipeline: " + ex.Message);
                }
            }
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void LoadRegions(string[] filenames)
        {
            foreach (var f in filenames)
            {
                HObject r;
                var name = Path.GetFileName(f);
                HOperatorSet.ReadRegion(out r, f);
                var split = name.Split('_');
                listROIs.Items.Add(new ROIEntry(int.Parse(split[1].Replace(".hobj", "")), split[0], r));
            }
        }

        private void UpdateIndices()
        {
            for (int i = 0; i < listROIs.Items.Count; i++)
            {
                var entry = (ROIEntry)listROIs.Items[i];
                entry.Index = i;
            }

            typeof(ListBox).InvokeMember("RefreshItems",
             BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod,
             null, listROIs, new object[] { }); // wtf
        }

        private bool AllEntriesLabelled()
        {
            for (int i = 0; i < listROIs.Items.Count; i++)
            {
                var entry = (ROIEntry)listROIs.Items[i];
                if (string.IsNullOrEmpty(entry.ROIType)) return false;
            }
            return true;
        }

        private void newRegionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (image == null || drawing) return;
            var thread = new Thread(DrawRegion);
            drawing = true;
            thread.Start();
        }

        private void DrawRegion()
        {
            HObject region = hWindowControl1.HalconWindow.DrawRegion();
            HOperatorSet.FillUp(region, out region);
            var idx = 0;
            listROIs.Invoke((Action)(() =>
            {
                idx = listROIs.Items.Count;
                var entry = new ROIEntry(idx, "", region);
                listROIs.Items.Add(entry);

            }));

            Invoke((Action)(() =>
            {
                DisplayImage();
            }));

            Invoke((Action)(() =>
            {
                drawing = false;
            }));
        }
        
        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            if (image == null) return;
            imgX = (int)(hScrollBar1.Value / 100.0 * (double)imgWidth);
            DisplayImage();
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            if (image == null) return;
            imgY = (int)(vScrollBar1.Value / 100.0 * (double)imgHeight);
            DisplayImage();
        }

    }
}
