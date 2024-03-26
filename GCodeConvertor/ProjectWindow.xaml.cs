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
using Point = System.Windows.Point;

namespace GCodeConvertor
{
    /// <summary>
    /// Логика взаимодействия для ProjectWindow.xaml
    /// </summary>

    public partial class ProjectWindow : Window
    {
        public enum DrawingStates{
            SET_START_POINT,
            DRAWING,
            SET_END_POINT
        }

        private double ELLIPSE_SIZE = 5;
        private double LINE_SIZE = 2;

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
        List<Ellipse> layerEllipses;
        List<Ellipse> selectedEllipses;
        Layer activeLayer;

        LayerStorage storage;

        ObservableCollection<CustomItem> ItemsList { get; set; }

        Point startPointSelectionRect;
        System.Windows.Shapes.Rectangle selectionRect;

        //Для редактирования точек
        private bool isDraggingEllipse;
        private Point offset;

        CustomLineStorage lineStorage;

        Line fLine;
        Line sLine;

        //Для ctrl + z

        Stack<Screen> screens = new Stack<Screen>();

        public ProjectWindow()
        {
            InitializeComponent();
            DataContext = ProjectSettings.preset;
            line = new Line();
            drawingState = DrawingStates.SET_START_POINT;
            layerPoints = new List<System.Windows.Point>();
            storage = new LayerStorage();
            layerEllipses = new List<Ellipse>();
            selectedEllipses = new List<Ellipse>();
            PreviewKeyDown += Window_KeyDown;

            ItemsList = new ObservableCollection<CustomItem>();
            layerListBox.ItemsSource = ItemsList;

            lineStorage = new CustomLineStorage();
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
                    {
                        MessageBox.Show("Слой является законченным");
                        break;
                    }

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
                    ellipse.Height = ELLIPSE_SIZE;
                    ellipse.Width = ELLIPSE_SIZE;
                    ellipse.Fill = new SolidColorBrush(Colors.Red);
                    ellipse.MouseRightButtonDown += Ellipse_MouseRightDown;
                    ellipse.MouseLeftButtonDown += Ellipse_MouseLeftButtonDown;
                    ellipse.MouseMove += Ellipse_MouseMove;
                    Canvas.SetLeft(ellipse, endX - ELLIPSE_SIZE / 2);
                    Canvas.SetTop(ellipse, endY - ELLIPSE_SIZE / 2);
                    layerEllipses.Add(ellipse);

                    line.Fill = new SolidColorBrush(Colors.Red);
                    line.Visibility = System.Windows.Visibility.Visible;
                    line.StrokeThickness = LINE_SIZE;
                    line.Stroke = System.Windows.Media.Brushes.Red;
                    line.X1 = currentDotX;
                    line.Y1 = currentDotY;
                    line.X2 = endX;
                    line.Y2 = endY;
                    line.MouseRightButtonDown += Line_MouseRightButtonDown;
                    CanvasMain.Children.Add(line);
                    ((sender as System.Windows.Shapes.Rectangle).Parent as Canvas).Children.Add(ellipse);

                    currentDotX = endX;
                    currentDotY = endY;

                    CustomLine cuLine = new CustomLine(line, layerEllipses[layerEllipses.Count - 2], ellipse);
                    lineStorage.addLine(cuLine);

                    line = new Line();

                    layerPoints.Add(new System.Windows.Point((double)((int)Math.Floor((e.GetPosition(CanvasMain).X / size)) + 0.5),
                                                                    (double)((int)Math.Floor(e.GetPosition(CanvasMain).Y / size) + 0.5)));

                    List<Point> currentPointsLayer = new List<Point>(activeLayer.layerThread);
                    Screen screen = new Screen(drawingState, drawArrow, currentPointsLayer);
                    screens.Push(screen);

                    activeLayer.layerThread.Add(new System.Windows.Point(currentDotX - ELLIPSE_SIZE / 2, currentDotY - ELLIPSE_SIZE / 2));

                }
            }
            else
            {
                currentDotX = Canvas.GetLeft(sender as System.Windows.Shapes.Rectangle) + size / 2;
                currentDotY = Canvas.GetTop(sender as System.Windows.Shapes.Rectangle) + size / 2;

                Ellipse ellipse = new Ellipse();
                ellipse.Height = ELLIPSE_SIZE;
                ellipse.Width = ELLIPSE_SIZE;
                ellipse.Fill = new SolidColorBrush(Colors.Red);
                ellipse.MouseRightButtonDown += Ellipse_MouseRightDown;
                ellipse.MouseLeftButtonDown += Ellipse_MouseLeftButtonDown;
                ellipse.MouseMove += Ellipse_MouseMove;
                Canvas.SetLeft(ellipse, currentDotX - ELLIPSE_SIZE / 2);
                Canvas.SetTop(ellipse, currentDotY - ELLIPSE_SIZE / 2);
                layerEllipses.Add(ellipse);
                ((sender as System.Windows.Shapes.Rectangle).Parent as Canvas).Children.Add(ellipse);

                layerPoints.Add(new System.Windows.Point((double)((int)Math.Floor((e.GetPosition(CanvasMain).X / size)) + 0.5),
                                                                (double)((int)Math.Floor(e.GetPosition(CanvasMain).Y / size) + 0.5)));

                List<Point> currentPointsLayer = new List<Point>(activeLayer.layerThread);
                Screen screen = new Screen(drawingState, drawArrow, currentPointsLayer);
                screens.Push(screen);

                drawArrow = true;

                activeLayer.layerThread.Add(new System.Windows.Point(currentDotX - ELLIPSE_SIZE / 2, currentDotY - ELLIPSE_SIZE / 2));
            }
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

            layerEllipses.Clear();
            selectedEllipses.Clear();
            drawArrow = false;
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

            screens.Clear();
            CustomItem selectedItemListBox = (CustomItem)layerListBox.SelectedItem;
            activeLayer = storage.getLayerByName(selectedItemListBox.LabelContent);
            selectedEllipses.Clear();
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
            layerEllipses.Clear();
            lineStorage.clear();
            if (activeLayer.layerThread != null)
            {
                int position = 0;
                System.Windows.Point startPoint;
                System.Windows.Point endPoint;

                foreach (System.Windows.Point point in activeLayer.layerThread)
                {
                    Ellipse ellipse = new Ellipse();
                    ellipse.Height = ELLIPSE_SIZE;
                    ellipse.Width = ELLIPSE_SIZE;
                    ellipse.Fill = new SolidColorBrush(Colors.Red);
                    ellipse.MouseRightButtonDown += Ellipse_MouseRightDown;
                    ellipse.MouseLeftButtonDown += Ellipse_MouseLeftButtonDown;
                    ellipse.MouseMove += Ellipse_MouseMove;
                    Canvas.SetLeft(ellipse, point.X);
                    Canvas.SetTop(ellipse, point.Y);
                    layerEllipses.Add(ellipse);

                    if (position == 0)
                    {
                        startPoint = point;
                    }
                    else
                    {
                        endPoint = point;
                        Line lineToAdd = buildLine(startPoint, endPoint);
                        CanvasMain.Children.Add(lineToAdd);
                        startPoint = endPoint;
                        CustomLine cuLine = new CustomLine(lineToAdd, layerEllipses[layerEllipses.IndexOf(ellipse) - 1], ellipse);
                        lineStorage.addLine(cuLine);
                    }
                    CanvasMain.Children.Add(ellipse);
                    position++;
                }
            }
        }

        private Line buildLine(System.Windows.Point startPoint, System.Windows.Point endPoint)
        {
            Line lineAdd = new Line();
            lineAdd.Fill = new SolidColorBrush(Colors.Red);
            lineAdd.Visibility = System.Windows.Visibility.Visible;
            lineAdd.StrokeThickness = LINE_SIZE;
            lineAdd.Stroke = System.Windows.Media.Brushes.Red;
            lineAdd.X1 = startPoint.X + ELLIPSE_SIZE / 2;
            lineAdd.Y1 = startPoint.Y + ELLIPSE_SIZE / 2;
            lineAdd.X2 = endPoint.X + ELLIPSE_SIZE / 2;
            lineAdd.Y2 = endPoint.Y + ELLIPSE_SIZE / 2;
            lineAdd.MouseRightButtonDown += Line_MouseRightButtonDown;
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

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.A))
            {
                selectAllEllipses();
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.Delete))
            {
                hardDelete();
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.I))
            {
                selectInterval();
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.Z))
            {
                if (screens.Count == 0)
                {
                    return;
                }

                Screen screen = screens.Pop();
                activeLayer.layerThread = screen.layerThread;
                clearTable();
                loadActiveLayer(null);
                drawingState = screen.drawingState;
                drawArrow = screen.drawArrow;

                if (drawArrow)
                {
                    currentDotX = activeLayer.layerThread[activeLayer.layerThread.Count - 1].X + ELLIPSE_SIZE / 2;
                    currentDotY = activeLayer.layerThread[activeLayer.layerThread.Count - 1].Y + ELLIPSE_SIZE / 2;
                }
            }

            switch (e.Key)
            {
                case Key.Delete:
                    {
                        foreach (Ellipse el in selectedEllipses)
                        {
                            layerEllipses.RemoveAll(item => item.Equals(el));
                        }
                        clearTable();
                        repaintTable();
                        selectedEllipses.Clear();
                        break;
                    }
                case Key.Q: 
                    {
                        string s1 = "";
                        string s2 = "";

                        List<UIElement> elementsToRemove = CanvasMain.Children
                        .OfType<UIElement>()
                        .Where(e => e is Ellipse)
                        .ToList();

                        foreach (UIElement el in elementsToRemove) 
                        {
                            s1 += Canvas.GetLeft(el) + "-" + Canvas.GetTop(el) + " ";
                        }

                        foreach (System.Windows.Point p in activeLayer.layerThread)
                        {
                            s2 += p.X + "-" + p.Y + " ";
                        }
                        MessageBox.Show(s1 + " \n asdasdasd " + s2);
                        break;
                    }
            }
        }

        private void selectInterval()
        {
            if (selectedEllipses.Count >= 2)
            {
                List<int> positions = new List<int>();
                foreach (Ellipse el in selectedEllipses)
                {
                    positions.Add(layerEllipses.IndexOf(el));
                }

                int minPosition = positions.Min();
                int maxPosition = positions.Max();

                for (int i = minPosition + 1; i < maxPosition; i++)
                {
                    Ellipse el = layerEllipses[i];
                    if (!selectedEllipses.Contains(el))
                    {
                        selectedEllipses.Add(el);
                        el.Fill = new SolidColorBrush(Colors.Blue);
                    }
                }
            }
        }

        private void hardDelete()
        {
            if (selectedEllipses.Count != 0)
            {
                List<int> positions = new List<int>();
                foreach (Ellipse el in selectedEllipses)
                {
                    positions.Add(layerEllipses.IndexOf(el));
                }
                int start = positions.Min();
                layerEllipses.RemoveRange(start, layerEllipses.Count - start);
                clearTable();
                repaintTable();
                selectedEllipses.Clear();
            }
        }

        private void selectAllEllipses()
        {
            if (layerEllipses.Skip(1).Except(selectedEllipses).Count() == 0)
            {
                foreach (Ellipse el in layerEllipses)
                {
                    el.Fill = new SolidColorBrush(Colors.Red);
                }
                selectedEllipses.Clear();
                return;
            }

            foreach (Ellipse el in layerEllipses) 
            {
                if (!selectedEllipses.Contains(el) && layerEllipses.IndexOf(el) != 0)
                {
                    selectedEllipses.Add(el);
                    el.Fill = new SolidColorBrush(Colors.Blue);
                }
            }
        }

        private void repaintTable() 
        {
            List<Point> currentPointsLayer = new List<Point>(activeLayer.layerThread);
            Screen screen = new Screen(drawingState, drawArrow, currentPointsLayer);
            screens.Push(screen);

            int position = 0;
            List<System.Windows.Point> points = new List<System.Windows.Point>();
            bool isInsert = false;
            List<UIElement> wrongElements = new List<UIElement>();
            int wrongIndex = 0;
            lineStorage.clear();

            foreach (Ellipse el in layerEllipses)
            {
                if (isInsert)
                {
                    el.Fill = new SolidColorBrush(Colors.Gray);
                    wrongElements.Add(el);
                }
                points.Add(new System.Windows.Point(Canvas.GetLeft(el), Canvas.GetTop(el)));

                if (position != 0)
                {
                    if (!isInsert)
                    {
                        LineGeometry lineGeometry = new LineGeometry(new System.Windows.Point(Canvas.GetLeft(layerEllipses[position - 1]), Canvas.GetTop(layerEllipses[position - 1])), new System.Windows.Point(Canvas.GetLeft(el), Canvas.GetTop(el)));

                        foreach (System.Windows.Shapes.Rectangle rectangle in rectangles)
                        {
                            RectangleGeometry rectangleGeometry = new RectangleGeometry(new Rect(Canvas.GetLeft(rectangle), Canvas.GetTop(rectangle), rectangle.Width, rectangle.Height));
                            isInsert = lineGeometry.FillContainsWithDetail(rectangleGeometry) != IntersectionDetail.Empty;
                            if (isInsert)
                            {
                                el.Fill = new SolidColorBrush(Colors.Gray);
                                wrongIndex = layerEllipses.IndexOf(el);
                                wrongElements.Add(el);
                                break;
                            }
                        }
                    }
                    
                    Line lineAdd = new Line();
                    lineAdd.Fill = new SolidColorBrush(Colors.Red);
                    lineAdd.Visibility = System.Windows.Visibility.Visible;
                    lineAdd.StrokeThickness = LINE_SIZE;
                    lineAdd.X1 = Canvas.GetLeft(layerEllipses[position-1]) + ELLIPSE_SIZE / 2;
                    lineAdd.Y1 = Canvas.GetTop(layerEllipses[position-1]) + ELLIPSE_SIZE / 2;
                    lineAdd.X2 = Canvas.GetLeft(el) + ELLIPSE_SIZE / 2;
                    lineAdd.Y2 = Canvas.GetTop(el) + ELLIPSE_SIZE / 2;
                    lineAdd.MouseRightButtonDown += Line_MouseRightButtonDown;
                    if (isInsert)
                    {
                        lineAdd.Stroke = System.Windows.Media.Brushes.Gray;
                        wrongElements.Add(lineAdd);
                    }
                    else
                    {
                        lineAdd.Stroke = System.Windows.Media.Brushes.Red;
                    }
                    CanvasMain.Children.Add(lineAdd);
                    CustomLine cuLine = new CustomLine(lineAdd, layerEllipses[position - 1], el);
                    lineStorage.addLine(cuLine);
                }
                CanvasMain.Children.Add(el);

                position++;
            }

            List<System.Windows.Point> currentPoints = activeLayer.layerThread;
            activeLayer.layerThread = points;
            storage.getLayerByName(activeLayer.name).layerThread = points;

            if (isInsert)
            {
                MessageBoxResult result = MessageBox.Show("При удалении точки, сопло проходит через иглу. Отмените удаление, чтобы вернуть траекторию в исходное состояние, " +
                                                      "либо подтвердите удаление. При подтверждении удаления, путь, отмеченный серым цветом, будет удален.", "Конфликт при удалении", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

                if (result == MessageBoxResult.OK)
                {
                    layerEllipses.RemoveRange(wrongIndex, layerEllipses.Count - wrongIndex);
                    points.RemoveRange(wrongIndex, points.Count - wrongIndex);
                    foreach (UIElement el in wrongElements)
                    {
                        CanvasMain.Children.Remove(el);
                    }
                    MessageBox.Show(layerEllipses.Count.ToString());
                    if (!layerEllipses[0].Equals(layerEllipses[layerEllipses.Count - 1]))
                    {
                        drawingState = DrawingStates.DRAWING;
                        currentDotX = (int)points[points.Count - 1].X + ELLIPSE_SIZE / 2;
                        currentDotY = (int)points[points.Count - 1].Y + ELLIPSE_SIZE / 2;
                    }

                    //activeLayer.layerThread = points;
                    //storage.getLayerByName(activeLayer.name).layerThread = points;
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    screens.Pop();
                    activeLayer.layerThread = currentPoints;
                    storage.getLayerByName(activeLayer.name).layerThread = currentPoints;
                    clearTable();
                    loadActiveLayer(null);
                }
            }

            if (drawingState == DrawingStates.SET_END_POINT && !((Canvas.GetTop(layerEllipses[0]) == Canvas.GetTop(layerEllipses[layerEllipses.Count - 1])) && (Canvas.GetLeft(layerEllipses[0]) == Canvas.GetLeft(layerEllipses[layerEllipses.Count - 1]))) || layerEllipses.Count == 1)
            {
                drawingState = DrawingStates.DRAWING;
            }

            if (drawingState == DrawingStates.DRAWING)
            {
                currentDotX = (int)points[points.Count - 1].X + ELLIPSE_SIZE / 2;
                currentDotY = (int)points[points.Count - 1].Y + ELLIPSE_SIZE / 2;
            }
        }

        private void clearTable() 
        {
            List<UIElement> elementsToRemove = CanvasMain.Children
            .OfType<UIElement>()
            .Where(e => e is Line || e is Ellipse)
            .ToList();

            foreach (UIElement el in elementsToRemove)
            {
                CanvasMain.Children.Remove(el);
            }
        }

        private void Ellipse_MouseRightDown(object sender, MouseButtonEventArgs e)
        {
            Ellipse ellipse = (Ellipse)sender;
            if (layerEllipses[0].Equals(ellipse))
            {
                return;
            }
            if (selectedEllipses.Contains(ellipse))
            {
                selectedEllipses.Remove(ellipse);
                ellipse.Fill = new SolidColorBrush(Colors.Red);
            }
            else
            {
                selectedEllipses.Add(ellipse);
                ellipse.Fill = new SolidColorBrush(Colors.Blue);
            }
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.ButtonState == MouseButtonState.Pressed)
            {
                foreach (Ellipse el in layerEllipses)
                {
                    el.Fill = new SolidColorBrush(Colors.Red);
                }
                selectedEllipses.Clear();

                startPoint = e.GetPosition(CanvasMain);
                selectionRect = new System.Windows.Shapes.Rectangle
                {
                    StrokeThickness = 2,
                    Fill = new SolidColorBrush(Colors.Blue) { Opacity = 0.3 },
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(selectionRect, startPoint.X);
                Canvas.SetTop(selectionRect, startPoint.Y);
                CanvasMain.Children.Add(selectionRect);
                CanvasMain.CaptureMouse();
            }
        }

        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Released)
            {
                CanvasMain.ReleaseMouseCapture();
                CanvasMain.Children.Remove(selectionRect);
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.RightButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(CanvasMain);

                double left = Math.Min(startPoint.X, currentPoint.X);
                double top = Math.Min(startPoint.Y, currentPoint.Y);

                double width = Math.Abs(startPoint.X - currentPoint.X);
                double height = Math.Abs(startPoint.Y - currentPoint.Y);

                selectionRect.Width = width;
                selectionRect.Height = height;

                Canvas.SetLeft(selectionRect, left);
                Canvas.SetTop(selectionRect, top);
                selectEllipsesInsideSelectionRectangle(left, top, width, height);
            }
        }

        private void selectEllipsesInsideSelectionRectangle(double left, double top, double width, double height) 
        {
            foreach (Ellipse el in layerEllipses)
            {
                double ellipseLeft = Canvas.GetLeft(el);
                double ellipseTop = Canvas.GetTop(el);

                if (ellipseLeft >= left && ellipseLeft <= left + width &&
                        ellipseTop >= top && ellipseTop <= top + height)
                {
                    if (!selectedEllipses.Contains(el) && layerEllipses.IndexOf(el) != 0)
                    {
                        selectedEllipses.Add(el);
                        el.Fill = new SolidColorBrush(Colors.Blue);
                    }
                }
                else
                {
                    selectedEllipses.Remove(el);
                    el.Fill = new SolidColorBrush(Colors.Red);
                }
            }
        }

        private void Ellipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isDraggingEllipse)
            {
                var nellipse = sender as Ellipse;
                isDraggingEllipse = false;
                nellipse.ReleaseMouseCapture();
                clearTable();
                repaintTable();
                return;
            }

            var ellipse = sender as Ellipse;

            if (ellipse.Equals(layerEllipses[0]))
            {
                return;
            }

            CustomLine[] cuLines = lineStorage.getLinesByEllipse(ellipse);
            if (cuLines != null)
            {
                if (cuLines[0] != null)
                {
                    fLine = cuLines[0].line;
                }

                if (cuLines[1] != null)
                {
                    sLine = cuLines[1].line;
                }
            }
            else
            {
                MessageBox.Show("Внутренняя ошибка");
            }

            isDraggingEllipse = true;
            offset = e.GetPosition(ellipse);
            ellipse.CaptureMouse();
        }

        private void Ellipse_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingEllipse)
            {
                var ellipse = sender as Ellipse;
                var position = e.GetPosition(CanvasMain);
                Canvas.SetLeft(ellipse, position.X - offset.X);
                Canvas.SetTop(ellipse, position.Y - offset.Y);

                if (fLine != null)
                {
                    UpdateLinePosition(fLine, ellipse, false);
                }

                if (sLine != null)
                {
                    UpdateLinePosition(sLine, ellipse, true);
                }
            }
        }

        private void UpdateLinePosition(Line lineL, Ellipse el, bool isSec)
        {
            var ellipseCenterX = Canvas.GetLeft(el) + el.Width / 2;
            var ellipseCenterY = Canvas.GetTop(el) + el.Height / 2;

            if (isSec)
            {
                lineL.X2 = ellipseCenterX;
                lineL.Y2 = ellipseCenterY;
                return;
            }
            lineL.X1 = ellipseCenterX;
            lineL.Y1 = ellipseCenterY;
        }

        private void Line_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

            if (!Keyboard.IsKeyDown(Key.LeftAlt))
            {
                return;
            }

            Line pressedLine = (Line)sender;
            CustomLine customLine = lineStorage.getLineByInnerLine(pressedLine);

            int position;
            int canv_position;
            int a;
            //
            Ellipse startEllipse;
            Ellipse secondEllipse;
            //
            position = lineStorage.indexOf(customLine);
            //
            if (lineStorage.indexOf(customLine) == 0)
            {
                startEllipse = layerEllipses[0];
            }
            else 
            {
                startEllipse = lineStorage.getByIndex(position - 1).secondEllipse;
            }

            if (lineStorage.indexOf(customLine) == lineStorage.size() - 1)
            {
                secondEllipse = layerEllipses[layerEllipses.Count - 1];
                a = layerEllipses.Count - 1;
            }
            else 
            {
                secondEllipse = lineStorage.getByIndex(position + 1).firstEllipse;
                a = layerEllipses.IndexOf(secondEllipse);
            }
            //

            canv_position = CanvasMain.Children.IndexOf(pressedLine);
            lineStorage.remove(customLine);
            CanvasMain.Children.Remove(pressedLine);

            Ellipse ellipse = new Ellipse();
            ellipse.Height = ELLIPSE_SIZE;
            ellipse.Width = ELLIPSE_SIZE;
            ellipse.Fill = new SolidColorBrush(Colors.Red);
            ellipse.MouseRightButtonDown += Ellipse_MouseRightDown;
            ellipse.MouseLeftButtonDown += Ellipse_MouseLeftButtonDown;
            ellipse.MouseMove += Ellipse_MouseMove;
            Canvas.SetLeft(ellipse, e.GetPosition(CanvasMain).X - ELLIPSE_SIZE / 2);
            Canvas.SetTop(ellipse, e.GetPosition(CanvasMain).Y - ELLIPSE_SIZE / 2);

            layerEllipses.Insert(a, ellipse);
            CanvasMain.Children.Add(ellipse);

            Line lineAddF = new Line();
            lineAddF.Fill = new SolidColorBrush(Colors.Red);
            lineAddF.Visibility = System.Windows.Visibility.Visible;
            lineAddF.StrokeThickness = LINE_SIZE;
            lineAddF.X1 = Canvas.GetLeft(startEllipse) + ELLIPSE_SIZE / 2;
            lineAddF.Y1 = Canvas.GetTop(startEllipse) + ELLIPSE_SIZE / 2;
            lineAddF.X2 = Canvas.GetLeft(ellipse) + ELLIPSE_SIZE / 2;
            lineAddF.Y2 = Canvas.GetTop(ellipse) + ELLIPSE_SIZE / 2;
            lineAddF.MouseRightButtonDown += Line_MouseRightButtonDown;

            Line lineAddS = new Line();
            lineAddS.Fill = new SolidColorBrush(Colors.Red);
            lineAddS.Visibility = System.Windows.Visibility.Visible;
            lineAddS.StrokeThickness = LINE_SIZE;
            lineAddS.X1 = Canvas.GetLeft(ellipse) + ELLIPSE_SIZE / 2;
            lineAddS.Y1 = Canvas.GetTop(ellipse) + ELLIPSE_SIZE / 2;
            lineAddS.X2 = Canvas.GetLeft(secondEllipse) + ELLIPSE_SIZE / 2;
            lineAddS.Y2 = Canvas.GetTop(secondEllipse) + ELLIPSE_SIZE / 2;
            lineAddS.MouseRightButtonDown += Line_MouseRightButtonDown;

            CustomLine newFLine = new CustomLine(lineAddF, startEllipse, ellipse);
            CustomLine newSLine = new CustomLine(lineAddS, ellipse, secondEllipse);

            lineStorage.addLine(newSLine, position);
            lineStorage.addLine(newFLine, position);

            clearTable();
            repaintTable();
        }

        
    }
}
