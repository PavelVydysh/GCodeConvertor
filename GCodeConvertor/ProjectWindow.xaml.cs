using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Drawing;
using Polenter.Serialization;
using System.Security.Policy;
using static System.Windows.Forms.LinkLabel;
using System.Collections.ObjectModel;
using Microsoft.Win32;

namespace GCodeConvertor
{
    /// <summary>
    /// Логика взаимодействия для ProjectWindow.xaml
    /// </summary>

    public partial class ProjectWindow : Window
    {
        private enum DrawingStates{
            SET_START_POINT,
            DRAWING,
            SET_END_POINT
        }

        private List<System.Windows.Shapes.Rectangle> rectangles = new List<System.Windows.Shapes.Rectangle>();

        private DrawingStates drawingState;

        private int startPointX;
        private int startPointY;

        private double currentDotX;
        private double currentDotY;
        double size;
        private bool drawArrow = false;
        Line line;

        List<System.Windows.Point> layerPoints;
        Layer activeLayer;

        LayerStorage storage;

        ObservableCollection<CustomItem> ItemsList { get; set; }
        public ProjectWindow()
        {
            InitializeComponent();
            DataContext = ProjectSettings.preset;
            line = new Line();
            drawingState = DrawingStates.SET_START_POINT;
            layerPoints = new List<System.Windows.Point>();
            storage = new LayerStorage();

            ItemsList = new ObservableCollection<CustomItem>();
            layerListBox.ItemsSource = ItemsList;
        }


        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int docY = (int)Math.Floor(e.GetPosition(CanvasMain).Y / size);
            int docX = (int)Math.Floor(e.GetPosition(CanvasMain).X / size);

            int dotType = ProjectSettings.preset.topology.map[docY, docX];

            switch (drawingState)
            {
                case DrawingStates.SET_START_POINT:
                    if(dotType == 2)
                    {
                        startPointX = docX;
                        startPointY = docY;
                        draw(sender, e, dotType);
                        drawingState = DrawingStates.DRAWING;
                    }
                    break;
                case DrawingStates.DRAWING:
                    if (dotType != 3)
                    {
                        draw(sender, e, dotType);

                        if (docY == startPointY && docX == startPointX)
                        {
                            drawingState = DrawingStates.SET_END_POINT;
                        }
                    }
                    break;
                case DrawingStates.SET_END_POINT:
                    MessageBox.Show("Слой является законченным");
                    break;

            }

            //startPointFlag
        }

        private void draw(object sender, MouseButtonEventArgs e, int dotType)
        {
            if (drawArrow)
            {
                double startX = currentDotX;
                double startY = currentDotY;

                double endX = Canvas.GetLeft(sender as System.Windows.Shapes.Rectangle) + size / 2;
                double endY = Canvas.GetTop(sender as System.Windows.Shapes.Rectangle) + size / 2;

                bool isInsert = false; 

                LineGeometry lineGeometry = new LineGeometry(new System.Windows.Point(startX, startY), new System.Windows.Point(endX, endY));

                foreach(System.Windows.Shapes.Rectangle rectangle in rectangles)
                {
                    RectangleGeometry rectangleGeometry = new RectangleGeometry(new Rect(Canvas.GetLeft(rectangle), Canvas.GetTop(rectangle), rectangle.Width, rectangle.Height));
                    isInsert = lineGeometry.FillContainsWithDetail(rectangleGeometry) != IntersectionDetail.Empty;
                    if (isInsert)
                    {
                        break;
                    }
                }

                if (!isInsert)
                {
                    Ellipse ellipse = new Ellipse();
                    ellipse.Height = 5;
                    ellipse.Width = 5;
                    ellipse.Fill = new SolidColorBrush(Colors.Red);
                    Canvas.SetLeft(ellipse, endX);
                    Canvas.SetTop(ellipse, endY);
                    ((sender as System.Windows.Shapes.Rectangle).Parent as Canvas).Children.Add(ellipse);

                    line.Fill = new SolidColorBrush(Colors.Red);
                    line.Visibility = System.Windows.Visibility.Visible;
                    line.StrokeThickness = 4;
                    line.Stroke = System.Windows.Media.Brushes.Red;
                    line.X1 = currentDotX;
                    line.Y1 = currentDotY;
                    line.X2 = endX;
                    line.Y2 = endY;
                    CanvasMain.Children.Add(line);
                    line = new Line();

                    currentDotX = endX;
                    currentDotY = endY;

                    layerPoints.Add(new System.Windows.Point((double)((int)Math.Floor((e.GetPosition(CanvasMain).X / size)) + 0.5),
                                                                    (double)((int)Math.Floor(e.GetPosition(CanvasMain).Y / size) + 0.5)));

                    activeLayer.layerThread.Add(new System.Windows.Point(e.GetPosition(CanvasMain).X, e.GetPosition(CanvasMain).Y));
                }
            }
            else
            {
                drawArrow = true;

                currentDotX = Canvas.GetLeft(sender as System.Windows.Shapes.Rectangle) + size / 2;
                currentDotY = Canvas.GetTop(sender as System.Windows.Shapes.Rectangle) + size / 2;

                Ellipse ellipse = new Ellipse();
                ellipse.Height = 5;
                ellipse.Width = 5;
                ellipse.Fill = new SolidColorBrush(Colors.Red);
                Canvas.SetLeft(ellipse, currentDotX);
                Canvas.SetTop(ellipse, currentDotY);
                ((sender as System.Windows.Shapes.Rectangle).Parent as Canvas).Children.Add(ellipse);

                layerPoints.Add(new System.Windows.Point((double)((int)Math.Floor((e.GetPosition(CanvasMain).X / size)) + 0.5),
                                                                (double)((int)Math.Floor(e.GetPosition(CanvasMain).Y / size) + 0.5)));

                activeLayer.layerThread.Add(new System.Windows.Point(e.GetPosition(CanvasMain).X, e.GetPosition(CanvasMain).Y));
            }
        }

        private void checkLineForDots()
        {

        }

        private void openMenu(object sender, RoutedEventArgs e)
        {

            DoubleAnimation anim = new DoubleAnimation();
            if (RightMenu.Width > 0)
            {
                anim.From = 250;
                anim.To = 0;
                anim.Duration = TimeSpan.FromSeconds(0.2);
                RightMenu.BeginAnimation(WidthProperty, anim);
                return;
            }

            anim.From = 0;
            anim.To = 250;
            anim.Duration = TimeSpan.FromSeconds(0.2);
            RightMenu.BeginAnimation(WidthProperty, anim);
        }

        private void Canvas_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (drawArrow)
            {
                line.Fill = new SolidColorBrush(Colors.Red);
                line.Visibility = System.Windows.Visibility.Visible;
                line.StrokeThickness = 4;
                line.Stroke = System.Windows.Media.Brushes.Red;
                line.X1 = currentDotX;
                line.Y1 = currentDotY;
                line.X2 = e.GetPosition(CanvasMain).X;
                line.Y2 = e.GetPosition(CanvasMain).Y;
            }
        }

        private void Canvas_(object sender, MouseButtonEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            ProjectSettings.preset.savePreset();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(drawingState == DrawingStates.SET_END_POINT)
            {
                List<Layer> layersToGenerate = new List<Layer>();
                foreach (Layer layer in storage.layers)
                {
                    if (layer.enable)
                    {
                        layersToGenerate.Add(layer);
                    }
                }

                if ((bool)manyCheck.IsChecked)
                {
                    List<Layer> tempList = new List<Layer>();
                    for (int i = 0; i < int.Parse(layerZ_Count.Text); i++)
                    {
                        tempList.AddRange(layersToGenerate);
                    }
                    layersToGenerate = tempList;
                }
                GCodeGenerator.generate(layersToGenerate);
            }
            else
            {
                MessageBox.Show("Закончите слой для генерации g-code'а!", "Невозможно сгенерировать g-code!");
            }
        }

        private void manyCheck_Checked(object sender, RoutedEventArgs e)
        {
            layerZ_Count.IsEnabled = true;
        }
        private void manyCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            layerZ_Count.IsEnabled = false;
        }
        private void CanvasMain_Loaded(object sender, RoutedEventArgs e)
        {
            size = (double)(CanvasMain.ActualWidth / ProjectSettings.preset.topology.map.GetLength(0));
            MessageBox.Show(ProjectSettings.preset.topology.map.Length.ToString());

            for (int i = 0; i < ProjectSettings.preset.topology.map.GetUpperBound(1) + 1; i++)
            {
                for (int j = 0; j < ProjectSettings.preset.topology.map.GetUpperBound(0) + 1; j++)
                {
                    System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle();
                    if (ProjectSettings.preset.topology.map[j, i] == 1)
                    {
                        rectangle.Fill = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFBE98")) ;
                    }
                    else if (ProjectSettings.preset.topology.map[j, i] == 2)
                    {
                        rectangle.Fill = new SolidColorBrush(Colors.Green);
                    }
                    else if (ProjectSettings.preset.topology.map[j, i] == 3)
                    {
                        rectangle.Fill = new SolidColorBrush(Colors.Black);
                    }
                    else
                    {
                        rectangle.Fill = new SolidColorBrush(Colors.White);
                    }
                    rectangle.MouseLeftButtonDown += Rectangle_MouseLeftButtonDown;
                    rectangle.Height = size;
                    rectangle.Width = size;
                    rectangle.StrokeThickness = 1;

                    if(ProjectSettings.preset.topology.map[j, i] == 3)
                    {
                        rectangles.Add(rectangle);
                    }

                    Canvas.SetTop(rectangle, size * j);
                    Canvas.SetLeft(rectangle, size * i);
                    CanvasMain.Children.Add(rectangle);
                }
            }
            activeLayer = new Layer();
            storage.addLayer(activeLayer);
            ItemsList.Add(new CustomItem(activeLayer.name, "12"));
            layerListBox.SelectedIndex = 0;
        }

        private void createLayer(object sender, RoutedEventArgs e)
        {
            if (drawingState != DrawingStates.SET_END_POINT)
            {
                MessageBox.Show("Текущий слой не закончен");
                return;
            }

            Layer layer = new Layer();
            storage.addLayer(layer);
            //layerListBox.Items.Add(layer.name);
            CustomItem currentItem = new CustomItem(layer.name, "12");
            ItemsList.Add(currentItem);
            layerListBox.SelectedItem = currentItem;
            activeLayer = layer;
            drawingState = DrawingStates.SET_START_POINT;
        }

        private void layerListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (drawingState != DrawingStates.SET_END_POINT && layerListBox.Items.Count != 1 && !((CustomItem)layerListBox.SelectedItem).Equals(getCustomItemEqualsLayer(activeLayer.name)))
            {
                layerListBox.SelectedItem = getCustomItemEqualsLayer(activeLayer.name);
                MessageBox.Show("Текущий слой не закончен");
                return;
            }

            List<object> toRemove = new List<object>();
            foreach (object o in CanvasMain.Children)
            {
                if (o is Ellipse || o is Line || o is LineGeometry)
                {
                    toRemove.Add(o);
                }
            }

            foreach (object o in toRemove)
            {
                CanvasMain.Children.Remove(o as UIElement);
            }

            CustomItem selectedItemListBox = (CustomItem)layerListBox.SelectedItem;
            activeLayer = storage.getLayerByName(selectedItemListBox.LabelContent);
            loadActiveLayer(sender);
        }

        private CustomItem getCustomItemEqualsLayer(string name)
        {
            foreach (CustomItem item in ItemsList)
            {
                if (item.LabelContent.Equals(name))
                {
                    return item;
                }
            }
            return null;
        }

        private void loadActiveLayer(object sender)
        {
            if (activeLayer.layerThread != null)
            {
                System.Windows.Point startPoint;
                System.Windows.Point endPoint;

                foreach (System.Windows.Point point in activeLayer.layerThread)
                {
                    Ellipse ellipse = new Ellipse();
                    ellipse.Height = 5;
                    ellipse.Width = 5;
                    ellipse.Fill = new SolidColorBrush(Colors.Red);
                    Canvas.SetLeft(ellipse, point.X);
                    Canvas.SetTop(ellipse, point.Y);
                    CanvasMain.Children.Add(ellipse);

                    if (activeLayer.layerThread.IndexOf(point) == 0)
                    {
                        startPoint = point;
                    }
                    else
                    {
                        endPoint = point;
                        Line lineToAdd = buildLine(startPoint, endPoint);
                        CanvasMain.Children.Add(lineToAdd);
                        startPoint = endPoint;
                    }
                }
            }
        }

        private Line buildLine(System.Windows.Point startPoint, System.Windows.Point endPoint)
        {
            Line lineAdd = new Line();
            lineAdd.Fill = new SolidColorBrush(Colors.Red);
            lineAdd.Visibility = System.Windows.Visibility.Visible;
            lineAdd.StrokeThickness = 4;
            lineAdd.Stroke = System.Windows.Media.Brushes.Red;
            lineAdd.X1 = startPoint.X;
            lineAdd.Y1 = startPoint.Y;
            lineAdd.X2 = endPoint.X;
            lineAdd.Y2 = endPoint.Y;
            return lineAdd;
        }

        private void editLayerHeight(object sender, RoutedEventArgs e)
        {
            Button but = (Button)sender;
            string layerName = but.Tag?.ToString();

            string textBoxValue = ((TextBox)((StackPanel)((Button)sender).Parent).Children[0]).Text;
            if (textBoxValue == "")
            {
                ((TextBox)((StackPanel)((Button)sender).Parent).Children[0]).Text = storage.getLayerByName(layerName).heightLayer.ToString();
                return;
            }
            if (textBoxValue[0] == ',')
            {
                ((TextBox)((StackPanel)((Button)sender).Parent).Children[0]).Text = storage.getLayerByName(layerName).heightLayer.ToString();
                return;
            }
            storage.getLayerByName(layerName).heightLayer = float.Parse(textBoxValue);
        }

        private void deleteLayer(object sender, RoutedEventArgs e)
        {
            if (ItemsList.Count == 1)
            {
                MessageBox.Show("Это единственный слой. Создайте еще один, чтобы удалить текущий.");
                return;
            }

            Button but = (Button)sender;
            string layerName = but.Tag?.ToString();

            CustomItem selectedItem = (CustomItem)layerListBox.SelectedItem;
            string currentLabelValue = selectedItem.LabelContent;

            if (layerName.Equals(currentLabelValue))
            {
                int index = layerListBox.SelectedIndex;
                if (index == 0)
                {
                    activeLayer = storage.layers[layerListBox.SelectedIndex + 1];
                    layerListBox.SelectedIndex = index + 1;
                }
                else
                {
                    activeLayer = storage.layers[layerListBox.SelectedIndex - 1];
                    layerListBox.SelectedIndex = index - 1;
                }
                drawingState = DrawingStates.SET_END_POINT;
            }

            storage.removeLayerByName(layerName);
            ItemsList.Remove(getCustomItemEqualsLayer(layerName));

        }

        private void enableLayer_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox box = (CheckBox)sender;
            string layerName = box.Tag?.ToString();
            storage.getLayerByName(layerName).enable = true;
        }

        private void enableLayer_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox box = (CheckBox)sender;
            string layerName = box.Tag?.ToString();
            storage.getLayerByName(layerName).enable = false;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (var ch in e.Text)
            {
                if (!char.IsDigit(ch) && ch != ',')
                {
                    e.Handled = true;
                }
            }
        }

        private void moveLayerUp(object sender, RoutedEventArgs e)
        {
            Button but = (Button)sender;
            string layerName = but.Tag?.ToString();
            CustomItem item = getCustomItemEqualsLayer(layerName);
            int currentIndex = ItemsList.IndexOf(item);

            if (currentIndex != 0)
            {
                ItemsList.Move(currentIndex, currentIndex - 1);

                Layer currentLayer = storage.layers[currentIndex];
                Layer movedLayer = storage.layers[currentIndex - 1];
                storage.layers[currentIndex] = movedLayer;
                storage.layers[currentIndex - 1] = currentLayer;
            }
        }

        private void moveLayerDown(object sender, RoutedEventArgs e)
        {
            Button but = (Button)sender;
            string layerName = but.Tag?.ToString();
            CustomItem item = getCustomItemEqualsLayer(layerName);
            int currentIndex = ItemsList.IndexOf(item);

            if (currentIndex != ItemsList.Count - 1)
            {
                ItemsList.Move(currentIndex, currentIndex + 1);

                Layer currentLayer = storage.layers[currentIndex];
                Layer movedLayer = storage.layers[currentIndex + 1];
                storage.layers[currentIndex] = movedLayer;
                storage.layers[currentIndex + 1] = currentLayer;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mw = new MainWindow();
            mw.Show();
            this.Close();
        }

        private double minScale = 1;

        private System.Windows.Point startPoint;
        private bool isDragging = false;
        private double translationFactor = 1.0;

        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                var canvas = sender as Canvas;

                // Сохраняем текущий масштаб канваса и прокрутку
                var previousScale = ((MatrixTransform)canvas.RenderTransform).Matrix.M11;

                // Получаем позицию курсора относительно Canvas
                var mousePosition = e.GetPosition(canvas);

                // Вычисляем коэффициент масштабирования
                double scale = e.Delta > 0 ? 1.1 : 0.9;
                scale = previousScale * scale;

                if (scale >= minScale)
                {
                    Matrix matrix = new Matrix();
                    matrix.ScaleAtPrepend(scale, scale, mousePosition.X, mousePosition.Y);
                    canvas.RenderTransform = new MatrixTransform(matrix);
                }

                // Применяем масштабирование к канвасу


                // Предотвращаем дальнейшее распространение события
                e.Handled = true;
            }
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                var canvas = sender as Canvas;
                startPoint = e.GetPosition(canvas);
                isDragging = true;
                canvas.CaptureMouse();
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && startPoint != null)
            {
                var canvas = sender as Canvas;
                var position = e.GetPosition(canvas);

                // Вычисляем разницу между текущей и начальной позицией мыши, умножая на фактор скорости
                double dx = (position.X - startPoint.X) * translationFactor;
                double dy = (position.Y - startPoint.Y) * translationFactor;

                // Получаем текущую матрицу трансформации канваса
                Matrix matrix = ((MatrixTransform)canvas.RenderTransform).Matrix;

                // Проверяем, не выходит ли канвас за пределы контейнера
                matrix.Translate(dx, dy);

                // Устанавливаем новую матрицу трансформации
                canvas.RenderTransform = new MatrixTransform(matrix);

                // Обновляем начальную позицию мыши
                startPoint = position;
            }
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle && isDragging)
            {
                var canvas = sender as Canvas;
                isDragging = false;
                canvas.ReleaseMouseCapture();
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "GCD files(*.gcd)|*.gcd",

            };

            if (openFileDialog.ShowDialog() == true)
            {
                string pathToPreset = openFileDialog.FileName;
                ProjectSettings.preset.loadPreset(openFileDialog.FileName);
                ProjectWindow pw = new ProjectWindow();
                pw.Show();
                this.Close();
            }
        }

    }
}
