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
            this.lv_Zhang = new ComponentOwl.BetterListView.BetterListView();
            this.betterListViewColumnHeader1 = new ComponentOwl.BetterListView.BetterListViewColumnHeader();
            this.betterListViewColumnHeader2 = new ComponentOwl.BetterListView.BetterListViewColumnHeader();
            this.betterListViewColumnHeader3 = new ComponentOwl.BetterListView.BetterListViewColumnHeader();
            this.betterListViewColumnHeader4 = new ComponentOwl.BetterListView.BetterListViewColumnHeader();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.button1 = new System.Windows.Forms.Button();
            this.lv_ZhangDetail = new ComponentOwl.BetterListView.BetterListView();
            this.betterListViewColumnHeader5 = new ComponentOwl.BetterListView.BetterListViewColumnHeader();
            this.betterListViewColumnHeader6 = new ComponentOwl.BetterListView.BetterListViewColumnHeader();
            this.betterListViewColumnHeader7 = new ComponentOwl.BetterListView.BetterListViewColumnHeader();
            this.betterListViewColumnHeader8 = new ComponentOwl.BetterListView.BetterListViewColumnHeader();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            ((System.ComponentModel.ISupportInitialize)(this.lv_Zhang)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lv_ZhangDetail)).BeginInit();
            this.SuspendLayout();
            // 
            // lv_Zhang
            // 
            this.lv_Zhang.Columns.Add(this.betterListViewColumnHeader1);
            this.lv_Zhang.Columns.Add(this.betterListViewColumnHeader2);
            this.lv_Zhang.Columns.Add(this.betterListViewColumnHeader3);
            this.lv_Zhang.Columns.Add(this.betterListViewColumnHeader4);
            this.lv_Zhang.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lv_Zhang.Location = new System.Drawing.Point(0, 0);
            this.lv_Zhang.Name = "lv_Zhang";
            this.lv_Zhang.Size = new System.Drawing.Size(1172, 167);
            this.lv_Zhang.TabIndex = 0;
            this.lv_Zhang.SelectedIndexChanged += new System.EventHandler(this.lv_Zhang_SelectedIndexChanged);
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
            this.splitContainer1.Location = new System.Drawing.Point(12, 12);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.lv_Zhang);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.lv_ZhangDetail);
            this.splitContainer1.Size = new System.Drawing.Size(1172, 327);
            this.splitContainer1.SplitterDistance = 167;
            this.splitContainer1.TabIndex = 2;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(0, 0);
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
            this.lv_ZhangDetail.Location = new System.Drawing.Point(0, 0);
            this.lv_ZhangDetail.Name = "lv_ZhangDetail";
            this.lv_ZhangDetail.Size = new System.Drawing.Size(1172, 156);
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
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.listView1.Location = new System.Drawing.Point(12, 345);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(1172, 203);
            this.listView1.TabIndex = 3;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Pattern";
            this.columnHeader1.Width = 198;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Camera";
            this.columnHeader2.Width = 154;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "meanDist";
            this.columnHeader3.Width = 135;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "reprojectionError";
            this.columnHeader4.Width = 176;
            // 
            // ZhangSimulationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1196, 589);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.button1);
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
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
    }
}