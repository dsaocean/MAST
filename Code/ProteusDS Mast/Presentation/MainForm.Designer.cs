namespace MAST.Presentation
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
            this.numericUpDownMaxNumConcurrentSims = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonLoadSims = new System.Windows.Forms.Button();
            this.buttonHaltAll = new System.Windows.Forms.Button();
            this.buttonPauseAll = new System.Windows.Forms.Button();
            this.buttonStartAll = new System.Windows.Forms.Button();
            this.massiveListView1 = new MAST.Presentation.Controls.MassiveListView();
            this.columnHeaderFolderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderState = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderProgress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFullFolder = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxNumConcurrentSims)).BeginInit();
            this.SuspendLayout();
            // 
            // numericUpDownMaxNumConcurrentSims
            // 
            this.numericUpDownMaxNumConcurrentSims.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numericUpDownMaxNumConcurrentSims.Location = new System.Drawing.Point(709, 15);
            this.numericUpDownMaxNumConcurrentSims.Maximum = new decimal(new int[] {
            32768,
            0,
            0,
            0});
            this.numericUpDownMaxNumConcurrentSims.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownMaxNumConcurrentSims.Name = "numericUpDownMaxNumConcurrentSims";
            this.numericUpDownMaxNumConcurrentSims.Size = new System.Drawing.Size(55, 20);
            this.numericUpDownMaxNumConcurrentSims.TabIndex = 21;
            this.numericUpDownMaxNumConcurrentSims.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownMaxNumConcurrentSims.ValueChanged += new System.EventHandler(this.numericUpDownMaxNumConcurrentSims_ValueChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(517, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(186, 13);
            this.label1.TabIndex = 20;
            this.label1.Text = "Maximum Number of Concurrent Sims:";
            // 
            // buttonLoadSims
            // 
            this.buttonLoadSims.Location = new System.Drawing.Point(12, 12);
            this.buttonLoadSims.Name = "buttonLoadSims";
            this.buttonLoadSims.Size = new System.Drawing.Size(75, 23);
            this.buttonLoadSims.TabIndex = 19;
            this.buttonLoadSims.Text = "Load Sims...";
            this.buttonLoadSims.UseVisualStyleBackColor = true;
            this.buttonLoadSims.Click += new System.EventHandler(this.buttonLoadSims_Click);
            // 
            // buttonHaltAll
            // 
            this.buttonHaltAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonHaltAll.Location = new System.Drawing.Point(691, 447);
            this.buttonHaltAll.Name = "buttonHaltAll";
            this.buttonHaltAll.Size = new System.Drawing.Size(75, 23);
            this.buttonHaltAll.TabIndex = 18;
            this.buttonHaltAll.Text = "Halt All";
            this.buttonHaltAll.UseVisualStyleBackColor = true;
            this.buttonHaltAll.Click += new System.EventHandler(this.buttonHaltAll_Click);
            // 
            // buttonPauseAll
            // 
            this.buttonPauseAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonPauseAll.Location = new System.Drawing.Point(93, 447);
            this.buttonPauseAll.Name = "buttonPauseAll";
            this.buttonPauseAll.Size = new System.Drawing.Size(75, 23);
            this.buttonPauseAll.TabIndex = 17;
            this.buttonPauseAll.Text = "Pause All";
            this.buttonPauseAll.UseVisualStyleBackColor = true;
            this.buttonPauseAll.Click += new System.EventHandler(this.buttonPauseAll_Click);
            // 
            // buttonStartAll
            // 
            this.buttonStartAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonStartAll.Location = new System.Drawing.Point(12, 447);
            this.buttonStartAll.Name = "buttonStartAll";
            this.buttonStartAll.Size = new System.Drawing.Size(75, 23);
            this.buttonStartAll.TabIndex = 16;
            this.buttonStartAll.Text = "Start All";
            this.buttonStartAll.UseVisualStyleBackColor = true;
            this.buttonStartAll.Click += new System.EventHandler(this.buttonStartAll_Click);
            // 
            // massiveListView1
            // 
            this.massiveListView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.massiveListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderFolderName,
            this.columnHeaderState,
            this.columnHeaderProgress,
            this.columnHeaderFullFolder});
            this.massiveListView1.FullRowSelect = true;
            this.massiveListView1.GridLines = true;
            this.massiveListView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.massiveListView1.Location = new System.Drawing.Point(12, 41);
            this.massiveListView1.Name = "massiveListView1";
            this.massiveListView1.Size = new System.Drawing.Size(754, 400);
            this.massiveListView1.TabIndex = 23;
            this.massiveListView1.UseCompatibleStateImageBehavior = false;
            this.massiveListView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderFolderName
            // 
            this.columnHeaderFolderName.Text = "Folder Name";
            this.columnHeaderFolderName.Width = 220;
            // 
            // columnHeaderState
            // 
            this.columnHeaderState.Text = "State";
            this.columnHeaderState.Width = 70;
            // 
            // columnHeaderProgress
            // 
            this.columnHeaderProgress.Text = "Progress";
            // 
            // columnHeaderFullFolder
            // 
            this.columnHeaderFullFolder.Text = "Full Folder";
            this.columnHeaderFullFolder.Width = 400;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(778, 482);
            this.Controls.Add(this.massiveListView1);
            this.Controls.Add(this.numericUpDownMaxNumConcurrentSims);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonLoadSims);
            this.Controls.Add(this.buttonHaltAll);
            this.Controls.Add(this.buttonPauseAll);
            this.Controls.Add(this.buttonStartAll);
            this.Name = "MainForm";
            this.Text = "ProteusDS MAST";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxNumConcurrentSims)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown numericUpDownMaxNumConcurrentSims;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonLoadSims;
        private System.Windows.Forms.Button buttonHaltAll;
        private System.Windows.Forms.Button buttonPauseAll;
        private System.Windows.Forms.Button buttonStartAll;
        private Controls.MassiveListView massiveListView1;
        private System.Windows.Forms.ColumnHeader columnHeaderFolderName;
        private System.Windows.Forms.ColumnHeader columnHeaderState;
        private System.Windows.Forms.ColumnHeader columnHeaderProgress;
        private System.Windows.Forms.ColumnHeader columnHeaderFullFolder;
    }
}