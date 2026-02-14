namespace JAXBase
{
    partial class JAXDebuggerForm
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
            components = new System.ComponentModel.Container();
            btnStep = new Button();
            btnCancel = new Button();
            btnResume = new Button();
            btnStepInto = new Button();
            lblWatch = new Label();
            txtWatch = new TextBox();
            splitter1 = new Splitter();
            splitContainer1 = new SplitContainer();
            lblLine = new Label();
            label5 = new Label();
            lblPrg = new Label();
            lblLevel = new Label();
            label3 = new Label();
            label2 = new Label();
            codebox = new RichTextBox();
            treeView1 = new TreeView();
            label1 = new Label();
            contextMenuStripTree = new ContextMenuStrip(components);
            removeWatchToolStripMenuItem = new ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            contextMenuStripTree.SuspendLayout();
            SuspendLayout();
            // 
            // btnStep
            // 
            btnStep.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnStep.Location = new Point(204, 409);
            btnStep.Name = "btnStep";
            btnStep.Size = new Size(95, 23);
            btnStep.TabIndex = 4;
            btnStep.Text = "&Step (F6)";
            btnStep.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Location = new Point(542, 409);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 5;
            btnCancel.Text = "&Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnResume
            // 
            btnResume.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnResume.Location = new Point(454, 409);
            btnResume.Name = "btnResume";
            btnResume.Size = new Size(75, 23);
            btnResume.TabIndex = 6;
            btnResume.Text = "&Resume";
            btnResume.UseVisualStyleBackColor = true;
            // 
            // btnStepInto
            // 
            btnStepInto.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnStepInto.Location = new Point(315, 409);
            btnStepInto.Name = "btnStepInto";
            btnStepInto.Size = new Size(95, 23);
            btnStepInto.TabIndex = 7;
            btnStepInto.Text = "S&tep Into (F8)";
            btnStepInto.UseVisualStyleBackColor = true;
            // 
            // lblWatch
            // 
            lblWatch.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblWatch.AutoSize = true;
            lblWatch.Location = new Point(7, 399);
            lblWatch.Name = "lblWatch";
            lblWatch.Size = new Size(66, 15);
            lblWatch.TabIndex = 8;
            lblWatch.Text = "Add Watch";
            // 
            // txtWatch
            // 
            txtWatch.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            txtWatch.Location = new Point(8, 416);
            txtWatch.Name = "txtWatch";
            txtWatch.Size = new Size(123, 23);
            txtWatch.TabIndex = 9;
            txtWatch.KeyDown += txtWatch_KeyDown;
            // 
            // splitter1
            // 
            splitter1.Location = new Point(0, 0);
            splitter1.Name = "splitter1";
            splitter1.Size = new Size(3, 441);
            splitter1.TabIndex = 10;
            splitter1.TabStop = false;
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(lblLine);
            splitContainer1.Panel1.Controls.Add(label5);
            splitContainer1.Panel1.Controls.Add(lblPrg);
            splitContainer1.Panel1.Controls.Add(lblLevel);
            splitContainer1.Panel1.Controls.Add(label3);
            splitContainer1.Panel1.Controls.Add(label2);
            splitContainer1.Panel1.Controls.Add(codebox);
            splitContainer1.Panel1.RightToLeft = RightToLeft.No;
            splitContainer1.Panel1MinSize = 150;
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(treeView1);
            splitContainer1.Panel2.Controls.Add(label1);
            splitContainer1.Panel2.RightToLeft = RightToLeft.No;
            splitContainer1.Panel2MinSize = 150;
            splitContainer1.Size = new Size(622, 384);
            splitContainer1.SplitterDistance = 200;
            splitContainer1.TabIndex = 11;
            // 
            // lblLine
            // 
            lblLine.Location = new Point(573, 27);
            lblLine.Name = "lblLine";
            lblLine.Size = new Size(42, 15);
            lblLine.TabIndex = 8;
            lblLine.Text = "label4";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(536, 26);
            label5.Name = "label5";
            label5.Size = new Size(32, 15);
            label5.TabIndex = 7;
            label5.Text = "Line:";
            // 
            // lblPrg
            // 
            lblPrg.Location = new Point(194, 26);
            lblPrg.Name = "lblPrg";
            lblPrg.Size = new Size(325, 19);
            lblPrg.TabIndex = 6;
            lblPrg.Text = "label5";
            // 
            // lblLevel
            // 
            lblLevel.Location = new Point(42, 27);
            lblLevel.Name = "lblLevel";
            lblLevel.Size = new Size(42, 15);
            lblLevel.TabIndex = 5;
            lblLevel.Text = "label4";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(137, 26);
            label3.Name = "label3";
            label3.Size = new Size(56, 15);
            label3.TabIndex = 4;
            label3.Text = "Program:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(5, 26);
            label2.Name = "label2";
            label2.Size = new Size(37, 15);
            label2.TabIndex = 3;
            label2.Text = "Level:";
            // 
            // codebox
            // 
            codebox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            codebox.BackColor = SystemColors.ControlLightLight;
            codebox.BorderStyle = BorderStyle.FixedSingle;
            codebox.Location = new Point(0, 44);
            codebox.Name = "codebox";
            codebox.ReadOnly = true;
            codebox.Size = new Size(622, 153);
            codebox.TabIndex = 2;
            codebox.Text = "";
            codebox.WordWrap = false;
            // 
            // treeView1
            // 
            treeView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            treeView1.Location = new Point(3, 22);
            treeView1.Name = "treeView1";
            treeView1.Size = new Size(619, 158);
            treeView1.TabIndex = 4;
            treeView1.KeyDown += treeView1_KeyDown_1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(2, 4);
            label1.Name = "label1";
            label1.Size = new Size(41, 15);
            label1.TabIndex = 3;
            label1.Text = "Watch";
            // 
            // contextMenuStripTree
            // 
            contextMenuStripTree.Items.AddRange(new ToolStripItem[] { removeWatchToolStripMenuItem });
            contextMenuStripTree.Name = "contextMenuStripTree";
            contextMenuStripTree.Size = new Size(155, 26);
            // 
            // removeWatchToolStripMenuItem
            // 
            removeWatchToolStripMenuItem.Name = "removeWatchToolStripMenuItem";
            removeWatchToolStripMenuItem.Size = new Size(154, 22);
            removeWatchToolStripMenuItem.Text = "Remove Watch";
            // 
            // JAXDebuggerForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(624, 441);
            Controls.Add(splitContainer1);
            Controls.Add(splitter1);
            Controls.Add(txtWatch);
            Controls.Add(lblWatch);
            Controls.Add(btnStepInto);
            Controls.Add(btnResume);
            Controls.Add(btnCancel);
            Controls.Add(btnStep);
            MinimumSize = new Size(640, 480);
            Name = "JAXDebuggerForm";
            Text = "JAXDebugger";
            Load += JAXDebuggerForm_Load;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            contextMenuStripTree.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button btnStep;
        private Button btnCancel;
        private Button btnResume;
        private Button btnStepInto;
        private Label lblWatch;
        private TextBox txtWatch;
        private Splitter splitter1;
        private SplitContainer splitContainer1;
        private RichTextBox codebox;
        private Label label1;
        private Label lblLevel;
        private Label label3;
        private Label label2;
        private Label lblPrg;
        private Label lblLine;
        private Label label5;
        private TreeView treeView1;
        private ContextMenuStrip contextMenuStripTree;
        private ToolStripMenuItem removeWatchToolStripMenuItem;
    }
}