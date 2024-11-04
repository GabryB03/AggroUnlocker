using FileUnlockingSharp;
using MetroSuite;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

public partial class MainForm : MetroForm
{
    public MainForm()
    {
        InitializeComponent();
    }

    private void guna2Button4_Click(object sender, System.EventArgs e)
    {
        listBox1.Items.Clear();
    }

    private void guna2Button5_Click(object sender, System.EventArgs e)
    {
        List<string> strings = new List<string>();

        foreach (string s in listBox1.SelectedItems)
        {
            strings.Add(s);
        }

        foreach (string s in strings)
        {
            listBox1.Items.Remove(s);
        }
    }

    private void guna2Button2_Click(object sender, System.EventArgs e)
    {
        if (openFileDialog1.ShowDialog().Equals(DialogResult.OK))
        {
            foreach (string path in openFileDialog1.FileNames)
            {
                if (!listBox1.Items.Contains(path.ToLower()))
                {
                    listBox1.Items.Add(path.ToLower());
                }
            }
        }
    }

    private void guna2Button3_Click(object sender, System.EventArgs e)
    {
        if (folderBrowserDialog1.ShowDialog().Equals(DialogResult.OK))
        {
            if (!listBox1.Items.Contains(folderBrowserDialog1.SelectedPath.ToLower()))
            {
                listBox1.Items.Add(folderBrowserDialog1.SelectedPath.ToLower());
            }
        }
    }

    private void guna2Button1_Click(object sender, System.EventArgs e)
    {
        List<string> paths = new List<string>();

        foreach (string path in listBox1.Items)
        {
            paths.Add(path);
        }

        foreach (string path in paths)
        {
            listBox1.Items.Remove(path);

            new Thread(() =>
            {
                try
                {
                    FileUnlocker.ForcefullyCompleteDeletePath(path);
                }
                catch
                {

                }
            }).Start();
        }
    }

    private void listBox1_DragEnter(object sender, DragEventArgs e)
    {
        HandleDragEnter(e);
    }

    private void listBox1_DragDrop(object sender, DragEventArgs e)
    {
        HandleDragDrop(e);
    }

    private void MainForm_DragEnter(object sender, DragEventArgs e)
    {
        HandleDragEnter(e);
    }

    private void MainForm_DragDrop(object sender, DragEventArgs e)
    {
        HandleDragDrop(e);
    }

    public void HandleDragEnter(DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effect = DragDropEffects.Copy;
        }
    }

    public void HandleDragDrop(DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] filePaths = ((string[])e.Data.GetData(DataFormats.FileDrop));

            foreach (string path in filePaths)
            {
                if (!listBox1.Items.Contains(path.ToLower()))
                {
                    listBox1.Items.Add(path.ToLower());
                }
            }
        }
    }
}