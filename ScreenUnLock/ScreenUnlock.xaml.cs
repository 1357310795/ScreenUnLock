using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Business.UI.Controls
{
    /// <summary>
    /// ScreenUnlock.xaml 的交互逻辑
    /// </summary>
    public partial class ScreenUnlock : UserControl
    {

        /// <summary>
        /// 保存选择用的所有圆点
        /// </summary>
        private readonly List<Ellipse> ellipseList = new List<Ellipse>();

        /// <summary>
        /// 当前点
        /// </summary>
        private Ellipse currentEllipse;

        /// <summary>
        /// 当前线
        /// </summary>
        private Line currentLine;

        #region 圆点大小
        /// <summary>
        /// 圆点的大小
        /// </summary>
        public static readonly DependencyProperty PointSizeProperty = DependencyProperty.Register("PointSize", typeof(double), typeof(ScreenUnlock), new FrameworkPropertyMetadata(15.0));

        /// <summary>
        /// 九宫格圆点的大小
        /// </summary>
        public double PointSize { get { return Convert.ToDouble(GetValue(PointSizeProperty)); } set { SetValue(PointSizeProperty, value); } }
        #endregion

        #region 圆点颜色
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(SolidColorBrush), typeof(ScreenUnlock));

        /// <summary>
        /// 圆点颜色
        /// </summary>
        public SolidColorBrush Color
        {
            get { return GetValue(ColorProperty) as SolidColorBrush; }
            set { SetValue(ColorProperty, value); }
        }
        #endregion

        #region 选中的颜色
        /// <summary>
        /// 选中的颜色
        /// </summary>
        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register("SelectedColor", typeof(SolidColorBrush), typeof(ScreenUnlock), new FrameworkPropertyMetadata(new SolidColorBrush(Colors.Green), new PropertyChangedCallback((s, e) =>
        {
            var t = s as ScreenUnlock;
            if (t.canvas.Children.Count > 9)
                for (int i = 9; i < t.canvas.Children.Count; i++)
                {
                    Shape item = t.canvas.Children[i] as Shape;
                    if (item is Line)
                        item.Stroke = e.NewValue as SolidColorBrush;
                    else if (item is Ellipse ellipse)
                        item.Fill = e.NewValue as SolidColorBrush;
                }
        })));

        /// <summary>
        /// 选中的颜色
        /// </summary>
        public SolidColorBrush SelectedColor
        {
            get { return GetValue(SelectedColorProperty) as SolidColorBrush; }
            set { SetValue(SelectedColorProperty, value); }
        }
        #endregion

        #region 自定义事件

        /// <summary>
        /// 绘制完成以后触发
        /// </summary>
        public static readonly RoutedEvent AfterDrawEvent = EventManager.RegisterRoutedEvent("AfterDraw", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ScreenUnlock));

        /// <summary>
        /// 绘制完成以后触发
        /// </summary>
        public event RoutedEventHandler AfterDraw
        {
            //将路由事件添加路由事件处理程序
            add { AddHandler(AfterDrawEvent, value); }
            //从路由事件处理程序中移除路由事件
            remove { RemoveHandler(AfterDrawEvent, value); }
        }
        #endregion

        /// <summary>
        /// 经过点的集合
        /// </summary>
        public static readonly DependencyProperty PointsProperty = DependencyProperty.Register("Points", typeof(List<int>), typeof(ScreenUnlock));

        /// <summary>
        /// 经过点的集合
        /// </summary>
        public List<int> Points
        {
            get { return GetValue(PointsProperty) as List<int>; }
            set { SetValue(PointsProperty, value); }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ScreenUnlock()
        {
            InitializeComponent();
            Points = new List<int>();
            this.Loaded += ScreenUnlock_Loaded;
            this.MouseDown += ScreenUnlock_MouseDown;
            this.MouseUp += ScreenUnlock_MouseUp;
            this.MouseMove += ScreenUnlock_MouseMove;
        }

        /// <summary>
        /// 鼠标左键弹起
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScreenUnlock_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (canvas.Children.Count > 9)
                foreach (Shape item in canvas.Children)
                    if (item is Line)
                        item.Stroke = SelectedColor;
                    else if (item is Ellipse ellipse && ellipseList.Contains(ellipse))
                        item.Fill = SelectedColor;
            canvas.Children.Remove(currentLine);
            currentEllipse = null;
            currentLine = null;
            RaiseEvent(new RoutedEventArgs(AfterDrawEvent));
        }

        /// <summary>
        /// 鼠标移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScreenUnlock_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //鼠标左键处于点击状态
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                //获取当前鼠标位置
                var point = e.GetPosition(this);

                ///当没有遇到圆点之前绘制跟随鼠标的线
                if (currentLine != null)
                {
                    canvas.Children.Remove(currentLine);
                    currentLine.X2 = point.X;
                    currentLine.Y2 = point.Y;
                    canvas.Children.Add(currentLine);
                }

                //线跟着移动
                if (VisualTreeHelper.HitTest(this, point).VisualHit is Ellipse ellipse && currentEllipse != null)
                {
                    var p1 = (Point)((dynamic)currentEllipse.Tag).Point;
                    var p = (Point)((dynamic)ellipse.Tag).Point;
                    if (p1 != p) //鼠标经过圆点
                    {
                        //如果不包含该圆点,一个点只能用一次
                        if (!ellipseList.Contains(ellipse))
                        {
                            //绘制当前点和上个点之间的连线
                            var t = new Line()
                            {
                                Stroke = Color,
                                StrokeThickness = PointSize / 2,
                                X1 = p1.X,
                                Y1 = p1.Y,
                                X2 = p.X,
                                Y2 = p.Y
                            };
                            //修改当前点
                            currentEllipse = ellipse;
                            ellipseList.Add(ellipse);
                            canvas.Children.Add(t);
                            Points.Add((int)((dynamic)ellipse.Tag).Location);
                            if (currentLine != null)
                            {
                                canvas.Children.Remove(currentLine);
                                currentLine.X1 = p.X;
                                currentLine.Y1 = p.Y;
                                currentLine.X2 = p.X;
                                currentLine.Y2 = p.Y;
                                canvas.Children.Add(currentLine);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 鼠标点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScreenUnlock_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                //每次点击都是重新绘制,清空除了九宫格的所有元素
                while (canvas.Children.Count > 9)
                    canvas.Children.RemoveAt(canvas.Children.Count - 1);
                ellipseList.Clear();
                currentEllipse = null;
                Points.Clear();
                //再次点击时需要先把颜色修改为初始化颜色
                foreach (Shape item in canvas.Children)
                    item.Fill = Color;
                //获取当前鼠标位置
                var point = e.GetPosition(this);
                //鼠标所在位置是否有圆点
                if (VisualTreeHelper.HitTest(this, point).VisualHit is Ellipse ellipse) //鼠标经过圆点
                {
                    currentEllipse = ellipse;
                    ellipseList.Add(ellipse);
                    Points.Add((int)((dynamic)ellipse.Tag).Location);
                    var p = (Point)((dynamic)currentEllipse.Tag).Point;
                    currentLine = new Line()
                    {
                        Stroke = Color,
                        StrokeThickness = PointSize / 2,
                        X1 = p.X,
                        Y1 = p.Y,
                        X2 = p.X,
                        Y2 = p.Y
                    };
                }
            }
        }

        /// <summary>
        /// Load事件,绘制九宫格
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScreenUnlock_Loaded(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();
            //为了保证正方形
            var distance = Math.Min(this.ActualWidth == 0 ? this.Width : this.ActualWidth, this.ActualHeight == 0 ? this.Height : this.ActualHeight) / 3;
            double left = (distance - PointSize) / 2;
            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    var x = j * distance + left;
                    var y = i * distance + left;
                    Ellipse ellipse = new Ellipse()
                    {
                        Width = PointSize,
                        Height = PointSize,
                        Fill = Color,
                        Tag = new
                        {
                            Point = new Point(x + PointSize / 2, y + PointSize / 2),
                            Location = i * 3 + j + 1
                        }
                    };
                    ellipse.SetValue(Canvas.LeftProperty, x);
                    ellipse.SetValue(Canvas.TopProperty, y);
                    Canvas.SetLeft(ellipse, x);
                    Canvas.SetTop(ellipse, y);
                    canvas.Children.Add(ellipse);
                }
            }
        }
    }
}
