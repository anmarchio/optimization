using System;
using HalconDotNet;

namespace PRIME.RegionMarker
{
    partial class RegionMarker
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.hWindowControl1 = new HalconDotNet.HWindowControl();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveRegionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadRegionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mouseSizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.x1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.x4ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.x10ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.keyScrollingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.regionColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectedColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.regionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newRegionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fillOnOffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hideLabelsHToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.segmentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.backgroundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.usingPipelineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadPipelineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.process1 = new System.Diagnostics.Process();
            this.hScrollBar1 = new System.Windows.Forms.HScrollBar();
            this.vScrollBar1 = new System.Windows.Forms.VScrollBar();
            this.listROIs = new System.Windows.Forms.ListBox();
            this.contextROI = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.currentImageLabel = new System.Windows.Forms.Label();
            this.commonPipelinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // hWindowControl1
            // 
            this.hWindowControl1.BackColor = System.Drawing.Color.Black;
            this.hWindowControl1.BorderColor = System.Drawing.Color.Black;
            this.hWindowControl1.ImagePart = new System.Drawing.Rectangle(0, 0, 640, 480);
            this.hWindowControl1.Location = new System.Drawing.Point(20, 87);
            this.hWindowControl1.Name = "hWindowControl1";
            this.hWindowControl1.Size = new System.Drawing.Size(1164, 863);
            this.hWindowControl1.TabIndex = 0;
            this.hWindowControl1.WindowSize = new System.Drawing.Size(1164, 863);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.regionsToolStripMenuItem,
            this.segmentToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(20, 60);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1485, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadImageToolStripMenuItem,
            this.saveRegionsToolStripMenuItem,
            this.loadRegionsToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            this.fileToolStripMenuItem.Click += new System.EventHandler(this.fileToolStripMenuItem_Click);
            // 
            // loadImageToolStripMenuItem
            // 
            this.loadImageToolStripMenuItem.Name = "loadImageToolStripMenuItem";
            this.loadImageToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.loadImageToolStripMenuItem.Text = "Load Image";
            this.loadImageToolStripMenuItem.Click += new System.EventHandler(this.loadImageToolStripMenuItem_Click);
            // 
            // saveRegionsToolStripMenuItem
            // 
            this.saveRegionsToolStripMenuItem.Name = "saveRegionsToolStripMenuItem";
            this.saveRegionsToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.saveRegionsToolStripMenuItem.Text = "Save Region(s)";
            this.saveRegionsToolStripMenuItem.Click += new System.EventHandler(this.saveRegionsToolStripMenuItem_Click);
            // 
            // loadRegionsToolStripMenuItem
            // 
            this.loadRegionsToolStripMenuItem.Name = "loadRegionsToolStripMenuItem";
            this.loadRegionsToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.loadRegionsToolStripMenuItem.Text = "Load Region(s)";
            this.loadRegionsToolStripMenuItem.Click += new System.EventHandler(this.loadRegionsToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mouseSizeToolStripMenuItem,
            this.keyScrollingToolStripMenuItem,
            this.regionColorToolStripMenuItem,
            this.selectedColorToolStripMenuItem,
            this.textColorToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // mouseSizeToolStripMenuItem
            // 
            this.mouseSizeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.x1ToolStripMenuItem,
            this.x4ToolStripMenuItem,
            this.x10ToolStripMenuItem});
            this.mouseSizeToolStripMenuItem.Name = "mouseSizeToolStripMenuItem";
            this.mouseSizeToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.mouseSizeToolStripMenuItem.Text = "Mouse Size";
            this.mouseSizeToolStripMenuItem.Visible = false;
            // 
            // x1ToolStripMenuItem
            // 
            this.x1ToolStripMenuItem.Name = "x1ToolStripMenuItem";
            this.x1ToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.x1ToolStripMenuItem.Text = "1x1";
            // 
            // x4ToolStripMenuItem
            // 
            this.x4ToolStripMenuItem.Name = "x4ToolStripMenuItem";
            this.x4ToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.x4ToolStripMenuItem.Text = "4x4";
            // 
            // x10ToolStripMenuItem
            // 
            this.x10ToolStripMenuItem.Name = "x10ToolStripMenuItem";
            this.x10ToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.x10ToolStripMenuItem.Text = "10x10";
            // 
            // keyScrollingToolStripMenuItem
            // 
            this.keyScrollingToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2,
            this.toolStripMenuItem3,
            this.toolStripMenuItem4});
            this.keyScrollingToolStripMenuItem.Name = "keyScrollingToolStripMenuItem";
            this.keyScrollingToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.keyScrollingToolStripMenuItem.Text = "Key Scrolling";
            this.keyScrollingToolStripMenuItem.Click += new System.EventHandler(this.keyScrollingToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(92, 22);
            this.toolStripMenuItem2.Text = "20";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(92, 22);
            this.toolStripMenuItem3.Text = "60";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.toolStripMenuItem3_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(92, 22);
            this.toolStripMenuItem4.Text = "120";
            this.toolStripMenuItem4.Click += new System.EventHandler(this.toolStripMenuItem4_Click);
            // 
            // regionColorToolStripMenuItem
            // 
            this.regionColorToolStripMenuItem.Name = "regionColorToolStripMenuItem";
            this.regionColorToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.regionColorToolStripMenuItem.Text = "Region Color";
            // 
            // selectedColorToolStripMenuItem
            // 
            this.selectedColorToolStripMenuItem.Name = "selectedColorToolStripMenuItem";
            this.selectedColorToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.selectedColorToolStripMenuItem.Text = "Selected Color";
            // 
            // textColorToolStripMenuItem
            // 
            this.textColorToolStripMenuItem.Name = "textColorToolStripMenuItem";
            this.textColorToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.textColorToolStripMenuItem.Text = "Text Color";
            // 
            // regionsToolStripMenuItem
            // 
            this.regionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newRegionToolStripMenuItem,
            this.fillOnOffToolStripMenuItem,
            this.hideLabelsHToolStripMenuItem});
            this.regionsToolStripMenuItem.Name = "regionsToolStripMenuItem";
            this.regionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.regionsToolStripMenuItem.Text = "Regions";
            // 
            // newRegionToolStripMenuItem
            // 
            this.newRegionToolStripMenuItem.Name = "newRegionToolStripMenuItem";
            this.newRegionToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.newRegionToolStripMenuItem.Text = "New Region [Space]";
            this.newRegionToolStripMenuItem.Click += new System.EventHandler(this.newRegionToolStripMenuItem_Click);
            // 
            // fillOnOffToolStripMenuItem
            // 
            this.fillOnOffToolStripMenuItem.Name = "fillOnOffToolStripMenuItem";
            this.fillOnOffToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.fillOnOffToolStripMenuItem.Text = "Fill [F]";
            this.fillOnOffToolStripMenuItem.Click += new System.EventHandler(this.fillOnOffToolStripMenuItem_Click);
            // 
            // hideLabelsHToolStripMenuItem
            // 
            this.hideLabelsHToolStripMenuItem.Name = "hideLabelsHToolStripMenuItem";
            this.hideLabelsHToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.hideLabelsHToolStripMenuItem.Text = "Hide Labels [H]";
            this.hideLabelsHToolStripMenuItem.Click += new System.EventHandler(this.hideLabelsHToolStripMenuItem_Click);
            // 
            // segmentToolStripMenuItem
            // 
            this.segmentToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.backgroundToolStripMenuItem,
            this.usingPipelineToolStripMenuItem});
            this.segmentToolStripMenuItem.Name = "segmentToolStripMenuItem";
            this.segmentToolStripMenuItem.Size = new System.Drawing.Size(66, 20);
            this.segmentToolStripMenuItem.Text = "Segment";
            // 
            // backgroundToolStripMenuItem
            // 
            this.backgroundToolStripMenuItem.Name = "backgroundToolStripMenuItem";
            this.backgroundToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.backgroundToolStripMenuItem.Text = "Fore-/Background";
            this.backgroundToolStripMenuItem.Visible = false;
            // 
            // usingPipelineToolStripMenuItem
            // 
            this.usingPipelineToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadPipelineToolStripMenuItem,
            this.commonPipelinesToolStripMenuItem});
            this.usingPipelineToolStripMenuItem.Name = "usingPipelineToolStripMenuItem";
            this.usingPipelineToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.usingPipelineToolStripMenuItem.Text = "Using Pipeline";
            // 
            // loadPipelineToolStripMenuItem
            // 
            this.loadPipelineToolStripMenuItem.Name = "loadPipelineToolStripMenuItem";
            this.loadPipelineToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.loadPipelineToolStripMenuItem.Text = "Load Pipeline";
            this.loadPipelineToolStripMenuItem.Click += new System.EventHandler(this.loadPipelineToolStripMenuItem_Click);
            // 
            // process1
            // 
            this.process1.StartInfo.Domain = "";
            this.process1.StartInfo.LoadUserProfile = false;
            this.process1.StartInfo.Password = null;
            this.process1.StartInfo.StandardErrorEncoding = null;
            this.process1.StartInfo.StandardOutputEncoding = null;
            this.process1.StartInfo.UserName = "";
            this.process1.SynchronizingObject = this;
            // 
            // hScrollBar1
            // 
            this.hScrollBar1.Location = new System.Drawing.Point(12, 953);
            this.hScrollBar1.Name = "hScrollBar1";
            this.hScrollBar1.Size = new System.Drawing.Size(1200, 41);
            this.hScrollBar1.TabIndex = 2;
            this.hScrollBar1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hScrollBar1_Scroll);
            // 
            // vScrollBar1
            // 
            this.vScrollBar1.Location = new System.Drawing.Point(1187, 87);
            this.vScrollBar1.Name = "vScrollBar1";
            this.vScrollBar1.Size = new System.Drawing.Size(25, 863);
            this.vScrollBar1.TabIndex = 3;
            this.vScrollBar1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vScrollBar1_Scroll);
            // 
            // listROIs
            // 
            this.listROIs.FormattingEnabled = true;
            this.listROIs.Location = new System.Drawing.Point(1214, 90);
            this.listROIs.Name = "listROIs";
            this.listROIs.Size = new System.Drawing.Size(128, 901);
            this.listROIs.TabIndex = 4;
            this.listROIs.SelectedIndexChanged += new System.EventHandler(this.listROIs_SelectedIndexChanged);
            // 
            // contextROI
            // 
            this.contextROI.Name = "contextROI";
            this.contextROI.Size = new System.Drawing.Size(61, 4);
            // 
            // currentImageLabel
            // 
            this.currentImageLabel.AutoSize = true;
            this.currentImageLabel.Location = new System.Drawing.Point(866, 3);
            this.currentImageLabel.Name = "currentImageLabel";
            this.currentImageLabel.Size = new System.Drawing.Size(73, 13);
            this.currentImageLabel.TabIndex = 5;
            this.currentImageLabel.Text = "Current Image";
            // 
            // commonPipelinesToolStripMenuItem
            // 
            this.commonPipelinesToolStripMenuItem.Name = "commonPipelinesToolStripMenuItem";
            this.commonPipelinesToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.commonPipelinesToolStripMenuItem.Text = "Common Pipelines";
            // 
            // RegionMarker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1525, 1011);
            this.Controls.Add(this.currentImageLabel);
            this.Controls.Add(this.listROIs);
            this.Controls.Add(this.vScrollBar1);
            this.Controls.Add(this.hScrollBar1);
            this.Controls.Add(this.hWindowControl1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "RegionMarker";
            this.Text = "RegionMarker";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private HalconDotNet.HWindowControl hWindowControl1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveRegionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Diagnostics.Process process1;
        private System.Windows.Forms.ToolStripMenuItem mouseSizeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem x1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem x4ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem x10ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem regionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadRegionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newRegionToolStripMenuItem;
        private System.Windows.Forms.HScrollBar hScrollBar1;
        private System.Windows.Forms.VScrollBar vScrollBar1;
        private System.Windows.Forms.ListBox listROIs;
        private System.Windows.Forms.ContextMenuStrip contextROI;
        private System.Windows.Forms.ToolStripMenuItem fillOnOffToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem keyScrollingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem regionColorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectedColorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem textColorToolStripMenuItem;
        private System.Windows.Forms.Label currentImageLabel;
        private System.Windows.Forms.ToolStripMenuItem hideLabelsHToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem segmentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem backgroundToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem usingPipelineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadPipelineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem commonPipelinesToolStripMenuItem;
    }
}