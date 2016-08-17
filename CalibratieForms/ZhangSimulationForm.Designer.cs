namespace CalibratieForms {
    partial class ZhangSimulationForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.lv_Zhang = new ComponentOwl.BetterListView.BetterListView();
            this.betterListViewColumnHeader1 = new ComponentOwl.BetterListView.BetterListViewColumnHeader();
            this.betterListViewColumnHeader2 = new ComponentOwl.BetterListView.BetterListViewColumnHeader();
            this.betterListViewColumnHeader3 = new ComponentOwl.BetterListView.BetterListViewColumnHeader();
            this.betterListViewColumnHeader4 = new ComponentOwl.BetterListView.BetterListViewColumnHeader();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.lv_ZhangDetail = new ComponentOwl.BetterListView.BetterListView();
            this.betterListViewColumnHeader5 = new ComponentOwl.BetterListView.BetterListViewColumnHeader();
            this.betterListViewColumnHeader6 = new ComponentOwl.BetterListView.BetterListViewColumnHeader();
            this.betterListViewColumnHeader7 = new ComponentOwl.BetterListView.BetterListViewColumnHeader();
            this.betterListViewColumnHeader8 = new ComponentOwl.BetterListView.BetterListViewColumnHeader();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.CameraInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.calibratedCameraInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cameraSimulationViewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.button3 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.lv_Zhang)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lv_ZhangDetail)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lv_Zhang
            // 
            this.lv_Zhang.CheckBoxes = ComponentOwl.BetterListView.BetterListViewCheckBoxes.TwoState;
            this.lv_Zhang.Columns.Add(this.betterListViewColumnHeader1);
            this.lv_Zhang.Columns.Add(this.betterListViewColumnHeader2);
            this.lv_Zhang.Columns.Add(this.betterListViewColumnHeader3);
            this.lv_Zhang.Columns.Add(this.betterListViewColumnHeader4);
            this.lv_Zhang.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lv_Zhang.GroupHeaderBehavior = ComponentOwl.BetterListView.BetterListViewGroupHeaderBehavior.None;
            this.lv_Zhang.HeaderStyle = ComponentOwl.BetterListView.BetterListViewHeaderStyle.Sortable;
            this.lv_Zhang.Location = new System.Drawing.Point(0, 0);
            this.lv_Zhang.MultiSelect = false;
            this.lv_Zhang.Name = "lv_Zhang";
            this.lv_Zhang.Size = new System.Drawing.Size(1196, 264);
            this.lv_Zhang.SortVirtual = true;
            this.lv_Zhang.TabIndex = 0;
            this.lv_Zhang.SelectedIndexChanged += new System.EventHandler(this.lv_Zhang_SelectedIndexChanged);
            this.lv_Zhang.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lv_Zhang_MouseDown);
            // 
            // betterListViewColumnHeader1
            // 
            this.betterListViewColumnHeader1.Name = "betterListViewColumnHeader1";
            this.betterListViewColumnHeader1.Text = "Pattern";
            this.betterListViewColumnHeader1.Width = 120;
            // 
            // betterListViewColumnHeader2
            // 
            this.betterListViewColumnHeader2.Name = "betterListViewColumnHeader2";
            this.betterListViewColumnHeader2.Text = "Camera";
            // 
            // betterListViewColumnHeader3
            // 
            this.betterListViewColumnHeader3.Name = "betterListViewColumnHeader3";
            this.betterListViewColumnHeader3.Text = "meanDist";
            // 
            // betterListViewColumnHeader4
            // 
            this.betterListViewColumnHeader4.Name = "betterListViewColumnHeader4";
            this.betterListViewColumnHeader4.Text = "ReprojectionErr";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.button3);
            this.splitContainer1.Panel1.Controls.Add(this.button2);
            this.splitContainer1.Panel1.Controls.Add(this.button1);
            this.splitContainer1.Panel1.Controls.Add(this.lv_Zhang);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.lv_ZhangDetail);
            this.splitContainer1.Size = new System.Drawing.Size(1196, 589);
            this.splitContainer1.SplitterDistance = 264;
            this.splitContainer1.TabIndex = 2;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(506, 3);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "oplossen";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(587, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(90, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "add simulation";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // lv_ZhangDetail
            // 
            this.lv_ZhangDetail.Columns.Add(this.betterListViewColumnHeader5);
            this.lv_ZhangDetail.Columns.Add(this.betterListViewColumnHeader6);
            this.lv_ZhangDetail.Columns.Add(this.betterListViewColumnHeader7);
            this.lv_ZhangDetail.Columns.Add(this.betterListViewColumnHeader8);
            this.lv_ZhangDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lv_ZhangDetail.HeaderStyle = ComponentOwl.BetterListView.BetterListViewHeaderStyle.Sortable;
            this.lv_ZhangDetail.Location = new System.Drawing.Point(0, 0);
            this.lv_ZhangDetail.MultiSelect = false;
            this.lv_ZhangDetail.Name = "lv_ZhangDetail";
            this.lv_ZhangDetail.Size = new System.Drawing.Size(1196, 321);
            this.lv_ZhangDetail.TabIndex = 1;
            this.lv_ZhangDetail.SelectedIndexChanged += new System.EventHandler(this.lv_ZhangDetail_SelectedIndexChanged);
            // 
            // betterListViewColumnHeader5
            // 
            this.betterListViewColumnHeader5.Name = "betterListViewColumnHeader5";
            this.betterListViewColumnHeader5.Text = "calibboard";
            // 
            // betterListViewColumnHeader6
            // 
            this.betterListViewColumnHeader6.Name = "betterListViewColumnHeader6";
            this.betterListViewColumnHeader6.Text = "Angle";
            // 
            // betterListViewColumnHeader7
            // 
            this.betterListViewColumnHeader7.Name = "betterListViewColumnHeader7";
            this.betterListViewColumnHeader7.Text = "Dist";
            // 
            // betterListViewColumnHeader8
            // 
            this.betterListViewColumnHeader8.Name = "betterListViewColumnHeader8";
            this.betterListViewColumnHeader8.Text = "ReprojectionErr";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CameraInfoToolStripMenuItem,
            this.calibratedCameraInfoToolStripMenuItem,
            this.cameraSimulationViewToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(200, 70);
            this.contextMenuStrip1.Closing += new System.Windows.Forms.ToolStripDropDownClosingEventHandler(this.contextMenuStrip1_Closing);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // CameraInfoToolStripMenuItem
            // 
            this.CameraInfoToolStripMenuItem.Name = "CameraInfoToolStripMenuItem";
            this.CameraInfoToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.CameraInfoToolStripMenuItem.Text = "camera info";
            // 
            // calibratedCameraInfoToolStripMenuItem
            // 
            this.calibratedCameraInfoToolStripMenuItem.Name = "calibratedCameraInfoToolStripMenuItem";
            this.calibratedCameraInfoToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.calibratedCameraInfoToolStripMenuItem.Text = "calibrated camera info";
            // 
            // cameraSimulationViewToolStripMenuItem
            // 
            this.cameraSimulationViewToolStripMenuItem.Name = "cameraSimulationViewToolStripMenuItem";
            this.cameraSimulationViewToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.cameraSimulationViewToolStripMenuItem.Text = "camera simulation view";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(683, 3);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(85, 23);
            this.button3.TabIndex = 2;
            this.button3.Text = "oplossenCeres";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // ZhangSimulationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1196, 589);
            this.Controls.Add(this.splitContainer1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ZhangSimulationForm";
            this.Text = "ZhangSimulationForm";
            ((System.ComponentModel.ISupportInitialize)(this.lv_Zhang)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.lv_ZhangDetail)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ComponentOwl.BetterListView.BetterListView lv_Zhang;
        private ComponentOwl.BetterListView.BetterListViewColumnHeader betterListViewColumnHeader1;
        private ComponentOwl.BetterListView.BetterListViewColumnHeader betterListViewColumnHeader2;
        private ComponentOwl.BetterListView.BetterListViewColumnHeader betterListViewColumnHeader3;
        private ComponentOwl.BetterListView.BetterListViewColumnHeader betterListViewColumnHeader4;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private ComponentOwl.BetterListView.BetterListView lv_ZhangDetail;
        private ComponentOwl.BetterListView.BetterListViewColumnHeader betterListViewColumnHeader5;
        private ComponentOwl.BetterListView.BetterListViewColumnHeader betterListViewColumnHeader6;
        private ComponentOwl.BetterListView.BetterListViewColumnHeader betterListViewColumnHeader7;
        private ComponentOwl.BetterListView.BetterListViewColumnHeader betterListViewColumnHeader8;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem CameraInfoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem calibratedCameraInfoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cameraSimulationViewToolStripMenuItem;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
    }
}