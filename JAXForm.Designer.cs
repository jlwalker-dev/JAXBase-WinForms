namespace JAXBase
{
    partial class FrmJAXBase
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pnlScreen = new Panel();
            txtBox = new TextBox();
            pnlScreen.SuspendLayout();
            SuspendLayout();
            // 
            // pnlScreen
            // 
            pnlScreen.Controls.Add(txtBox);
            pnlScreen.Location = new Point(3, 0);
            pnlScreen.Name = "pnlScreen";
            pnlScreen.Size = new Size(400, 400);
            pnlScreen.TabIndex = 1;
            // 
            // txtBox
            // 
            txtBox.Location = new Point(3, 3);
            txtBox.Name = "txtBox";
            txtBox.Size = new Size(131, 23);
            txtBox.TabIndex = 0;
            txtBox.TextChanged += txtBox_TextChanged;
            // 
            // FrmJAXBase
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(416, 418);
            Controls.Add(pnlScreen);
            IsMdiContainer = true;
            Name = "FrmJAXBase";
            StartPosition = FormStartPosition.CenterParent;
            Text = "JAXBase Command Box";
            FormClosing += FrmJAXBase_FormClosing;
            Load += FrmJAXBase_Load;
            SizeChanged += FrmJAXBase_SizeChanged;
            pnlScreen.ResumeLayout(false);
            pnlScreen.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlScreen;
        private TextBox txtBox;
    }
}
