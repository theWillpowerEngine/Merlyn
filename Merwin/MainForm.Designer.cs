namespace Merwin
{
    partial class MainForm
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
            this.wpfHost = new System.Windows.Forms.Integration.ElementHost();
            this.ide = new Merwin.IDE();
            this.SuspendLayout();
            // 
            // wpfHost
            // 
            this.wpfHost.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.wpfHost.Location = new System.Drawing.Point(12, 12);
            this.wpfHost.Name = "wpfHost";
            this.wpfHost.Size = new System.Drawing.Size(804, 466);
            this.wpfHost.TabIndex = 0;
            this.wpfHost.Child = this.ide;
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(828, 490);
            this.Controls.Add(this.wpfHost);
            this.Name = "MainForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Integration.ElementHost wpfHost;
        private IDE ide;
    }
}

