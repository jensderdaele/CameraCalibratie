namespace CalibratieForms {
    partial class CameraSelectionForm {
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
            this.betterListView1 = new ComponentOwl.BetterListView.BetterListView();
            ((System.ComponentModel.ISupportInitialize)(this.betterListView1)).BeginInit();
            this.SuspendLayout();
            // 
            // betterListView1
            // 
            this.betterListView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.betterListView1.Location = new System.Drawing.Point(0, 0);
            this.betterListView1.Name = "betterListView1";
            this.betterListView1.Size = new System.Drawing.Size(659, 380);
            this.betterListView1.TabIndex = 0;
            this.betterListView1.View = ComponentOwl.BetterListView.BetterListViewView.LargeIcon;
            // 
            // CameraSelectionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(659, 380);
            this.Controls.Add(this.betterListView1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "CameraSelectionForm";
            this.Text = "CameraSelectionForm";
            ((System.ComponentModel.ISupportInitialize)(this.betterListView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private ComponentOwl.BetterListView.BetterListView betterListView1;
    }
}