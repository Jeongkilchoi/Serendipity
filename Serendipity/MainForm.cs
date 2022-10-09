using SerendipityLibrary;
using Serendipity.Utilities;
using Util = Serendipity.Utilities.Utility;

namespace Serendipity
{
    public partial class MainForm : Form
    {
        #region 필드
        private bool _dragging = false;
        private int _mousePosX;
        private int _mousePosY;
        private int _lastOrder;
        private readonly CheckBox[] _conditionBoxes;
        private CancellationTokenSource _cts = null;
        #endregion

        #region 속성
        /// <summary>
        /// 제외번호 리스트
        /// </summary>
        public List<int> FixedList { get; set; } = new() { };
        /// <summary>
        /// 고정번호 리스트
        /// </summary>
        public List<int> ExceptList { get; set; } = new() { };
        #endregion

        public MainForm()
        {
            InitializeComponent();
            _lastOrder = Util.GetLastOrder();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            int a = 2;
            int b = 3;
            int c = a + b;
        }

        private void PictureBox_Paint(object sender, PaintEventArgs e)
        {
            var graphic = e.Graphics;
            graphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var lastNums = Util.DangbeonOfOrder(Util.GetLastOrder());

            //가로선 그리기
            for (int i = 0; i < 8; i++)
            {
                using var pen = new Pen(Color.Gray, 0.5F);
                var pt1 = new Point(25, 25 + (i * 50));
                var pt2 = i != 7 ? new Point(25 + (50 * 7), 25 + (i * 50)) :
                                   new Point(25 + (50 * 3), 25 + (i * 50));

                graphic.DrawLine(pen, pt1, pt2);
            }

            //세로선 그리기
            for (int i = 0; i < 8; i++)
            {
                using var pen = new Pen(Color.Gray, 0.5F);
                var pt1 = new Point(25 + (i * 50), 25);
                var pt2 = i < 4 ? new Point(25 + (i * 50), 25 + (50 * 7)) :
                                  new Point(25 + (i * 50), 25 + (50 * 6));

                graphic.DrawLine(pen, pt1, pt2);
            }

            int n = 0;
            var size = new Size(50, 50);
            var points = new List<Point>();
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    n++;

                    if (n <= 45)
                    {
                        //숫자 그리기
                        using var font = new Font("Swis721 Cn BT", 16, FontStyle.Regular, GraphicsUnit.Point);
                        var rect = new Rectangle(25 + (50 * j), 25 + (50 * i), size.Width, size.Height);
                        var rect1 = new Rectangle(25 + (50 * j) + 22, 25 + (50 * i) + 22, 6, 6);
                        StringFormat stringFormat = new()
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };

                        graphic.DrawString(n.ToString(), font, Brushes.Black, rect, stringFormat);

                        if (lastNums.Contains(n))
                        {
                            //최종당번이면 가운데 빨간점 그리기
                            var brush = new SolidBrush(Color.FromArgb(150, Color.Red));
                            graphic.FillEllipse(brush, rect1);

                            //가운 포인트 저장
                            var pt = new Point(rect.X + 25, rect.Y + 25);
                            points.Add(pt);
                        }

                        //제외수 포함되면 X 표시
                        if (ExceptList.Contains(n))
                        {
                            var exppts1 = new Point[2];
                            exppts1[0] = new Point(25 + (50 * j), 25 + (50 * i));
                            exppts1[1] = new Point(25 + (50 * j) + 50, 25 + (50 * i) + 50);
                            var exppts2 = new Point[2];
                            exppts2[0] = new Point(25 + (50 * j) + 50, 25 + (50 * i));
                            exppts2[1] = new Point(25 + (50 * j), 25 + (50 * i) + 50);

                            graphic.DrawLines(new Pen(Color.Red, 2), exppts1);
                            graphic.DrawLines(new Pen(Color.Red, 2), exppts2);
                        }

                        //고정수 포함되면 ㅁ 표시
                        if (FixedList.Contains(n))
                        {
                            var rect2 = new Rectangle(25 + (50 * j) + 5, 25 + (50 * i) + 5, 40, 40);
                            var brush = new SolidBrush(Color.FromArgb(100, Color.Black));
                            graphic.FillEllipse(brush, rect2);
                        }
                    }
                }
            }

            //당번번호 선잇기
            using (var danpen = new Pen(Color.Green, 1))
            {
                graphic.DrawLines(danpen, points.ToArray());
            }

            var seven = SimpleData.HorizontalFlowDatas(7);
            var idxpoint = lastNums.Select(x => Convexhull.IndexToPoint(x, seven)).ToList();
            var hull = Convexhull.GetConvexHull(idxpoint);
            hull.Add(hull.First());
            var hullpts = hull.Select(g => new Point(g.Y * 50 + 50, g.X * 50 + 50));
            //외곽선 채우기
            using (var hullbrush = new SolidBrush(Color.FromArgb(100, Color.Cyan)))
            {
                graphic.FillPolygon(hullbrush, hullpts.ToArray());
            }

            e.Dispose();
        }

        private void SaveCheckButton_Click(object sender, EventArgs e)
        {

        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void MinimumButton_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }




    }
}