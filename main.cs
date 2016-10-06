using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web.Script.Serialization;
using System.Diagnostics;

namespace EasyWorkshop
{
    class AddonJSONFile
    {
        public string title;
        public string type;
        public string[] tags;
        public string[] ignore = { "*.gma" };
    }

    class WorkshopPackage
    {
        public string title = null;
        public string path = null;
        public string icon = "test/replace/with/null.jpeg";
        public string type = null;
        public string[] tags = null;

        public bool IsValid()
        {
            if (title == null || path == null || icon == null || type == null || tags == null)
            {
                Console.WriteLine("ERROR:\tOne of these is not specified: title, folder, icon, type, tags");
                return false;
            }

            if ( tags.Length > 2 )
            {
                Console.WriteLine("ERROR:\tOnly up to 2 tags allowed!");
                return false;
            }

            try
            {
                string[] substrings = icon.Split('/');
                string fileName = substrings[substrings.Length - 1];
                string fileExtension = fileName.Split('.')[1];
                if (fileExtension != "jpg")
                {
                    Console.WriteLine("ERROR:\tWrong file extension - only jpeg allowed!");
                    return false;
                }
            }
            catch
            {
                Console.WriteLine("ERROR:\t Invalid icon selection!");
                return false;
            }


            return true;
        }

        public void Commit()
        {
            AddonJSONFile json = new AddonJSONFile();
            json.title = title;
            json.type = type;
            json.tags = tags;

            var result = new JavaScriptSerializer().Serialize(json);
            File.WriteAllText(path + @"\addon.json", result);

            Console.Clear();

            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("/----------------------------------------------------------------\n\tExecuting packing service\n----------------------------------------------------------------/\n");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Process gmad = new Process();
            gmad.StartInfo.FileName = @"content\executables\gmad.exe";
            gmad.StartInfo.Arguments = $"create -folder {path} -out {path}/package.gma";
            gmad.StartInfo.UseShellExecute = false;
            gmad.StartInfo.RedirectStandardOutput = true;
            gmad.Start();
            Console.WriteLine(gmad.StandardOutput.ReadToEnd());

            Thread.Sleep(1000);

            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("/----------------------------------------------------------------\n\tExecuting publishing service\n----------------------------------------------------------------/");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Process gmpublish = new Process();
            gmpublish.StartInfo.FileName = @"content\executables\gmpublish.exe";
            gmpublish.StartInfo.Arguments = $"create -icon {icon} -addon {path}/package.gma";
            gmpublish.StartInfo.UseShellExecute = false;
            gmpublish.StartInfo.RedirectStandardOutput = true;
            gmpublish.Start();

            Console.WriteLine( gmad.StandardOutput.ReadToEnd());

            Thread.Sleep(1000);
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\nRESULT:\tCheck above for errors!\n");
        }
    }

    class StylishForm : Form
    {
        public Color backgroundColor = Color.FromArgb(30, 30, 30);
        public string backgroundImage = null;
        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rect = new Rectangle(0, 0, Size.Width - 1, Size.Height - 1);
            Pen pen = new Pen(backgroundColor);
            SolidBrush brush = new SolidBrush(backgroundColor);

            e.Graphics.FillRectangle(brush, rect);
            e.Graphics.DrawRectangle(pen, rect);

            if( backgroundImage != null )
            {
                Image background = Image.FromFile(backgroundImage);
                e.Graphics.DrawImage(background, 0, 0, Size.Width - 1, Size.Height - 1);
            }

        }
    }

    class StylishTextEntry : TextBox
    {
        public StylishTextEntry()
        {
            //BackColor = Color.Transparent;
            Size = new Size(240, 40);
        }

        public Color OutlineColor = Color.FromArgb(40, 40, 40);
        public Color Background = Color.FromArgb(25, 25, 25);
        protected override void OnPaint(PaintEventArgs e)
        {
            Pen Outline = new Pen(OutlineColor, 1);
            SolidBrush Fill = new SolidBrush(Background);
            Rectangle Rect = new Rectangle(0, 0, Width-1, Height-1);

            e.Graphics.FillRectangle(Fill, Rect);
            e.Graphics.DrawRectangle(Outline, Rect);
        }
    }

    class StylishComboBox : ComboBox
    {
        //placeholder
    }

    class StylishButton : UserControl
    {
        public StylishButton()
        {
            //Override inherited values
            BackColor = Color.Transparent;
            Size = new Size(240, 40);
            Text = "Stylish Button";
        }

        //Register modifiers
        public Color Background = Color.Black;
        public Color SideGround = Color.Gray;
        public Color IconBackground = Color.LightGray;
        public Color TextColor = Color.Black;
        public Font TextFont = new Font("Arial", 16);
        public StringFormat TextFormat = new StringFormat();
        public int SideHeight = 4;
        public Image IconImage = null;

        public void SetIcon( string path )
        {
            IconImage = Image.FromFile(path);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //Background
            Pen Outline = new Pen(Background);
            SolidBrush Fill = new SolidBrush(Background);
            Rectangle Rect = new Rectangle(0, 0, Width-1, Height-1);

            e.Graphics.FillRectangle(Fill, Rect);
            e.Graphics.DrawRectangle(Outline, Rect);

            //SideGround
            Rect.Y = Height - SideHeight;
            Rect.Height = SideHeight;
            Outline.Color = SideGround;
            Fill.Color = SideGround;

            e.Graphics.DrawRectangle(Outline, Rect);
            e.Graphics.FillRectangle(Fill, Rect);

            //Icon
            if( IconImage != null )
            {
                Rect.Y = 0;
                Rect.Width = Rect.Height = (Height - SideHeight) - 1;
                Outline.Color = Fill.Color = IconBackground;

                e.Graphics.FillRectangle(Fill, Rect);
                e.Graphics.DrawRectangle(Outline, Rect);

                e.Graphics.DrawImage(IconImage, ( Height - SideHeight ) / 2 - IconImage.Height / 2, (Height - SideHeight) / 2 - IconImage.Height / 2);
            }

            //Text
            Fill.Color = TextColor;
            PointF TextPos = new PointF(Width / 2, (Height / 2) - SideHeight - TextFont.SizeInPoints / 2);
            TextFormat.Alignment = (IconImage != null) ? StringAlignment.Near : StringAlignment.Center;
            TextPos.X = (IconImage != null) ? (Height - SideHeight) + (Height - SideHeight) / 2 - IconImage.Height / 2 : (Width / 2);
            e.Graphics.DrawString(Text, TextFont, Fill, TextPos, TextFormat);
        }
    }

    class Program
    {
        [STAThread]
        static void Main()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;

            StylishForm mainWindow = new StylishForm();
            mainWindow.backgroundColor = Color.FromArgb(25, 30, 30);
            mainWindow.Text = "Easy Workshop";
            mainWindow.SetBounds(0, 0, 600, 400);
            mainWindow.FormBorderStyle = FormBorderStyle.FixedSingle;
            mainWindow.MaximizeBox = false;
            mainWindow.MinimizeBox = false;
            mainWindow.Icon = new Icon("content/icon.ico");

            Panel mainPanel = new Panel();
            mainPanel.Parent = mainWindow;
            mainPanel.SetBounds(0, 40, 2400, 360);
            mainPanel.BackColor = Color.Transparent;
            mainPanel.ForeColor = Color.Transparent;

            Label welcomeMessage = new Label();
            welcomeMessage.TextAlign = ContentAlignment.MiddleCenter;
            welcomeMessage.Font = new Font("Arial", 12);
            welcomeMessage.Text = "Welcome to Easy Workshop!\nUse menu above to navigate between functionalities.";
            welcomeMessage.Parent = mainPanel;
            welcomeMessage.SetBounds(0, 0, 600, 320);

            StylishButton upload = new StylishButton();
            upload.Text = "Upload";
            upload.Parent = mainWindow;
            upload.SetBounds(0, 0, 200, 30);
            upload.SetIcon("content/icons/uploadicon.png");
            upload.TextFont = new Font("Arial", 12);
            upload.Background = Color.FromArgb(86, 182, 90);
            upload.SideGround = Color.FromArgb(61, 139, 64);
            upload.IconBackground = Color.FromArgb(71, 163, 75);
            upload.TextColor = Color.White;
            upload.Click += (sender, e) => mainPanel.Left = -600;

            Label titleLabel = new Label();
            titleLabel.TextAlign = ContentAlignment.TopCenter;
            titleLabel.Font = new Font("Arial", 10);
            titleLabel.Text = "Title: ";
            titleLabel.Parent = mainPanel;
            titleLabel.AutoSize = true;
            titleLabel.Top = 20;
            titleLabel.Left = 930;

            StylishTextEntry title = new StylishTextEntry();
            title.Parent = mainPanel;
            title.SetBounds(930, 45, 200, 25);

            Label typeLabel = new Label();
            typeLabel.TextAlign = ContentAlignment.TopCenter;
            typeLabel.Font = new Font("Arial", 10);
            typeLabel.Text = "Type: ";
            typeLabel.Parent = mainPanel;
            typeLabel.AutoSize = true;
            typeLabel.Top = 80;
            typeLabel.Left = 930;

            string[] typeOptions = { "ServerContent", "gamemode", "map", "weapon", "vehicle", "npc", "tool", "effects", "model" };
            StylishComboBox type = new StylishComboBox();
            type.Parent = mainPanel;
            type.Size = new Size(200, 25);
            type.Top = 105;
            type.Left = 930;
            for (int i = 0; i < typeOptions.Length; i++)
            {
                type.Items.Add(typeOptions[i]);
            }

            string[] tagCloud = { "fun", "roleplay", "scenic", "movie", "realism", "cartoon", "water", "comic", "build" };
            CheckBox[] tagCheckboxes = new CheckBox[tagCloud.Length];
            Label tagsLabel = new Label();
            tagsLabel.TextAlign = ContentAlignment.TopCenter;
            tagsLabel.Font = new Font("Arial", 10);
            tagsLabel.Text = "Tags: ";
            tagsLabel.Parent = mainPanel;
            tagsLabel.AutoSize = true;
            tagsLabel.Top = 170;
            tagsLabel.Left = 930;
            for (int i = 0; i < tagCloud.Length; i++)
            {
                tagCheckboxes[i] = new CheckBox();
                tagCheckboxes[i].Parent = mainPanel;
                tagCheckboxes[i].Text = tagCloud[i];
                tagCheckboxes[i].Top = 190 + 20 * ( i % 5 );
                tagCheckboxes[i].Left = 930 + 140 * ((i / 5) % 2);
            }

            //Folder to upload
            Label fileLabel = new Label();
            fileLabel.TextAlign = ContentAlignment.TopCenter;
            fileLabel.Font = new Font("Arial", 10);
            fileLabel.Text = "Folder: ";
            fileLabel.Parent = mainPanel;
            fileLabel.AutoSize = true;
            fileLabel.Top = 12;
            fileLabel.Left = 615;

            StylishButton fileFolder = new StylishButton();
            fileFolder.Text = "Select";
            fileFolder.Parent = mainPanel;
            fileFolder.SetBounds(765, 5, 100, 30);
            fileFolder.TextFont = new Font("Arial", 12);
            fileFolder.Background = Color.FromArgb(255, 152, 0);
            fileFolder.SideGround = Color.FromArgb(221, 132, 0);
            fileFolder.TextColor = Color.White;
            FolderBrowserDialog fileSelection = new FolderBrowserDialog();
            fileFolder.Click += (sender, e) =>
            {
                fileSelection.ShowDialog();
            };

            //Icon
            PictureBox iconPreview = new PictureBox();
            iconPreview.Parent = mainPanel;
            iconPreview.SizeMode = PictureBoxSizeMode.StretchImage;
            iconPreview.SetBounds(676, 115, 128, 128);

            Label iconLabel = new Label();
            iconLabel.TextAlign = ContentAlignment.TopCenter;
            iconLabel.Font = new Font("Arial", 10);
            iconLabel.Text = "Icon: ";
            iconLabel.Parent = mainPanel;
            iconLabel.AutoSize = true;
            iconLabel.Top = 52;
            iconLabel.Left = 615;

            Color[] availableColors = { Color.FromArgb(103, 58, 183), Color.FromArgb(76, 175, 80), Color.FromArgb(121, 85, 72), Color.FromArgb(244, 67, 54) };
            int iconId = 1;
            StylishButton iconGenerate = new StylishButton();
            iconGenerate.Text = "Generate";
            iconGenerate.Parent = mainPanel;
            iconGenerate.SetBounds(660, 45, 100, 30);
            iconGenerate.TextFont = new Font("Arial", 12);
            iconGenerate.Background = Color.FromArgb(63, 81, 181);
            iconGenerate.SideGround = Color.FromArgb(54, 70, 156);
            iconGenerate.TextColor = Color.White;
            iconGenerate.Click += (sender, e) =>
            {
                Bitmap img = new Bitmap(512, 512);
                Graphics grp = Graphics.FromImage(img);
                Random rnd = new Random();
                StringFormat format = new StringFormat();

                int clr = rnd.Next(0, availableColors.Length);
                grp.Clear(availableColors[clr]);
                format.Alignment = StringAlignment.Center;
                grp.DrawString(title.Text, new Font("Arial", 48), Brushes.White, 256, 256-48, format);
                grp.Dispose();

                img.Save($"{fileSelection.SelectedPath}/package_icon_{iconId}.jpg", ImageFormat.Jpeg );
                img.Dispose();

                iconPreview.Image = Image.FromFile($"{fileSelection.SelectedPath}/package_icon_{iconId}.jpg");
                Console.WriteLine("Icon generated in selected folder using specified title!");

                iconId++;
            };

            StylishButton iconFile = new StylishButton();
            iconFile.Text = "Select";
            iconFile.Parent = mainPanel;
            iconFile.SetBounds(765, 45, 100, 30);
            iconFile.TextFont = new Font("Arial", 12);
            iconFile.Background = Color.FromArgb(255, 152, 0);
            iconFile.SideGround = Color.FromArgb(221, 132, 0);
            iconFile.TextColor = Color.White;
            OpenFileDialog iconSelection = new OpenFileDialog();
            iconSelection.FileOk += (sender, e) => 
            {
                iconPreview.Image = Image.FromFile( iconSelection.FileName );
                if( iconPreview.Image.Size.Width != 512 || iconPreview.Image.Size.Height != 512 )
                {
                    iconSelection.FileName = null;
                    iconPreview.Image = null;
                    Console.WriteLine("ERROR:\tImage has to be no bigger and no smaller than 512x512!");
                }
            };

            iconSelection.Filter = "(*.JPG)|*.JPG;";
            iconFile.Click += (sender, e) =>
            {
                iconSelection.ShowDialog();
            };

            //Finalize, run checks for validation first :P
            StylishButton finish = new StylishButton();
            finish.Text = "Finish";
            finish.Parent = mainPanel;
            finish.SetBounds(615, 275, 250, 30);
            finish.TextFont = new Font("Arial", 12);
            finish.Background = Color.FromArgb(233, 30, 99);
            finish.SideGround = Color.FromArgb(209, 20, 84);
            finish.TextColor = Color.White;
            finish.Click += (sender, e) =>
            {
                WorkshopPackage package = new WorkshopPackage();
                package.title = title.Text;
                package.path = fileSelection.SelectedPath;
                package.icon = iconSelection.FileName;
                if (type.SelectedIndex != -1)
                {
                    package.type = typeOptions[type.SelectedIndex];
                }

                List<string> checkedTags = new List<string>();
                for ( int i = 0; i < tagCheckboxes.Length; i++ )
                {
                    if(tagCheckboxes[i].Checked)
                    {
                        checkedTags.Add( tagCheckboxes[i].Text );
                    }
                }
                package.tags = checkedTags.ToArray();

                if (package.IsValid())
                {
                    package.Commit();
                }
            };

            //Edit tab, edit already existing workshop elements
            StylishButton edit = new StylishButton();
            edit.Text = "Manage";
            edit.Parent = mainWindow;
            edit.SetBounds(200, 0, 200, 30);
            edit.SetIcon("content/icons/pencilicon.png");
            edit.TextFont = new Font("Arial", 12);
            edit.Background = Color.FromArgb(138, 118, 231);
            edit.SideGround = Color.FromArgb(100, 85, 170);
            edit.IconBackground = Color.FromArgb(126, 103, 217);
            edit.TextColor = Color.White;
            edit.Click += (sender, e) => mainPanel.Left = -1200;

            StylishButton about = new StylishButton();
            about.Text = "About";
            about.Parent = mainWindow;
            about.SetBounds(400, 0, 200, 30);
            about.SetIcon("content/icons/helpicon.png");
            about.TextFont = new Font("Arial", 12);
            about.Background = Color.FromArgb(223, 85, 83);
            about.SideGround = Color.FromArgb(198, 68, 66);
            about.IconBackground = Color.FromArgb(219, 77, 75);
            about.TextColor = Color.White;
            about.Click += (sender, e) => mainPanel.Left = -1800;

            Label aboutPage = new Label();
            aboutPage.TextAlign = ContentAlignment.MiddleCenter;
            aboutPage.Font = new Font("Arial", 10);
            aboutPage.Text = "Easy Workshop version 0.2";
            aboutPage.Parent = mainPanel;
            aboutPage.SetBounds(1800, 300, 600, 20);

            Label authorPage = new Label();
            authorPage.TextAlign = ContentAlignment.TopCenter;
            authorPage.Font = new Font("Arial", 20);
            authorPage.Text = "Brought to you by:";
            authorPage.ForeColor = Color.FromArgb(216, 172, 88);
            authorPage.Parent = mainPanel;
            authorPage.SetBounds(1800, 70, 600, 40);

            PictureBox devLogo = new PictureBox();
            devLogo.Parent = mainPanel;
            devLogo.Image = Image.FromFile("content/dev_logo.png");
            devLogo.SizeMode = PictureBoxSizeMode.StretchImage;
            devLogo.SetBounds(1800 + 250, 320 / 2 - 50, 100, 100);

            mainWindow.ShowDialog();
        }
    }
}
