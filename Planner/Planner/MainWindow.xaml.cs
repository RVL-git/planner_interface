using System;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;

namespace Planner
{
    //скролл с помощью мышки
    public partial class MainWindow : Window
    {
        
        Dictionary<DateTime, List<string>> plans;//храним все записи в виде словаря, ключ - дата, записи - лист из строк
        DateTime cur = DateTime.Now; // дата, которая сейчас используется ( для вывода дел на эту дату, для создания новой записи)
        List<string> curlist = new List<string>(); // список дел для этой даты
        string path = "plans.txt"; //путь к файлу (лучше не трогать файл вообще, оно работает все само), planner/bin/debug
        public MainWindow()
        {
            InitializeComponent();
            //выводим планы на сегодняшний день
            plans = get_plans(path);
            ShowList(plans[GetCur(cur)]);
            curlist = plans[GetCur(cur)];
            ShowList(plans[GetCur(cur)]);
        }
        
        //метод для преобразования даты в нужный нам формат (по сути костыль, который делает время у даты 00:00:00), необходимо, чтобы обращаться к словарю
        public DateTime GetCur(DateTime dt)
        {
            return DateTime.Parse(dt.ToShortDateString());
        }

        //получаем словарь из всех записей из файла
        //каждая запись имеет формат: dd.mm.yy/часы:минуты_текст запланированного дела_код для определения того, выполненно ли дело(0 или 1)//следующее дело
        public Dictionary<DateTime, List<string>> get_plans(string path)
        {
            Dictionary<DateTime, List<string>> temp = new Dictionary<DateTime, List<string>>();

            try
            {
                using (StreamReader fs = new StreamReader(path))
                {
                    string s = "";
                    s = fs.ReadLine();
                    while (s != null)
                    {
                        //в цикле парсим нашу строку для каждого дня
                        List<string> list = new List<string>();
                        var t = s.Split('/');
                        int size = t.Length;
                        DateTime date = DateTime.Parse(t[0]);
                        if (date < GetCur(cur))
                            break;
                        for (int i = 1; i < size; i++)
                            list.Add(t[i]);

                        temp.Add(date, list);
                        s = fs.ReadLine();
                    }
                }
            }
            catch
            {
                MessageBox.Show("Упс, что-то не так с входным файлом, проверьте!"); 
            }
            return temp;
        }

        //метод для отображения списка дел для опред. дня
        private void ShowList(List<string> list)
        {
            //добавляем нужное кол-во строк в grid, добавляем нужные элементы и прописываем им обработчики событий
            try
            {
                for (int i = 0; i < list.Count; i++)
                {

                    var s = list[i].Split('_');
                    main.RowDefinitions.Add(new RowDefinition());
                    main.RowDefinitions[i].Height = new GridLength(60);

                    TextBox block = new TextBox();
                    block.TextWrapping = TextWrapping.Wrap;

                    if (s[0] != "0")
                    {
                        DateTime temp = DateTime.Parse(s[0]);
                        block.Text = temp.ToShortTimeString();
                    }

                    TextBox box = new TextBox();
                    box.TextWrapping = TextWrapping.Wrap;
                    box.Text = s[1];

                    CheckBox toggle = new CheckBox();
                    toggle.Width = 100;
                    toggle.Height = 100;
                    toggle.LayoutTransform = new ScaleTransform(4, 4);
                    toggle.Checked += Toggle_Checked;
                    toggle.Unchecked += Toggle_Checked;

                    if (Convert.ToInt32(s[2]) == 1)
                        toggle.IsChecked = true;
                    
                    
                    Button bt_delete = new Button();
                    bt_delete.Content = "удалить";
                    bt_delete.Click += delete_Click;

                    Button bt_save = new Button();
                    bt_save.Content = "сохранить";
                    bt_save.Click += save_Click;

                    main.Children.Add(bt_save);
                    main.Children.Add(bt_delete);
                    main.Children.Add(box);
                    main.Children.Add(toggle);
                    main.Children.Add(block);

                    Grid.SetColumn(block, 0);
                    Grid.SetRow(block, i);
                    Grid.SetColumn(box, 1);
                    Grid.SetRow(box, i);

                    Grid.SetColumn(bt_save, 2);
                    Grid.SetRow(bt_save, i);
                    Grid.SetColumn(bt_delete, 3);
                    Grid.SetRow(bt_delete, i);

                    Grid.SetColumn(toggle, 4);
                    Grid.SetRow(toggle, i);
                }
            }
            catch
            {
                //MessageBox.Show("Упс, что-то не так с входным файлом, проверьте!1");
            }
        }

        //метод, который реагирует на изменение checkbox(галочки), при измении обновляем словарь записей
        private void Toggle_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox bt = sender as CheckBox;
            var row = Grid.GetRow(bt);
            var temp = curlist[row];
            temp = temp.Substring(0, temp.Length - 1);
            if (bt.IsChecked == true)
                temp += "1";
            else
                temp += "0";
            curlist[row] = temp;

            plans[GetCur(cur)] = curlist;
        }

        //обработчик кнопки, которая добавляет новую запись, проверяем правильность формата времени, обновляем словарь
        private void add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (time.Text.Length != 5)
                {
                    MessageBox.Show("Введите время в правильном формате!(hh:mm)");
                    return;
                }
                var temp = time.Text.Split(':');
                if (temp.Length != 2)
                {
                    MessageBox.Show("Введите время в правильном формате!(hh:mm)");
                    return;
                }
                if (Convert.ToInt32(temp[0]) > 25 || Convert.ToInt32(temp[0]) < 0)
                {
                    MessageBox.Show("Введите время в правильном формате!(hh:mm)");
                    return;
                }
                if (Convert.ToInt32(temp[1]) > 60 || Convert.ToInt32(temp[1]) < 0)
                {
                    MessageBox.Show("Введите время в правильном формате!(hh:mm)");
                    return;
                }
                curlist.Add(time.Text + "_" + plan.Text + "_0");
                plans[GetCur(cur)] = curlist;

                MessageBox.Show("Запись добавлена!");
            }
            catch
            {
                MessageBox.Show("Запись не сохранена. Ошибка!");
            }
            time.Text = "";
            plan.Text = "";
        }

        //обработчики кнопок сохранить в каждой строке записи. после каждого изменения времени или текста нажимаем сохранить, чтобы обновить данные в словаре
        private void save_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;
            var row = Grid.GetRow(bt);
            var child = main.Children.OfType<UIElement>().ToList();
            string s = "";
            string time ="";
            foreach (UIElement element in child)
                if (Grid.GetRow(element) == row)
                {
                    if(Grid.GetColumn(element) == 0)
                    {
                        TextBox temp = (TextBox)element;
                        if (temp.Text == "")
                            time += "0_";
                        else
                        {
                            time += (temp.Text + "_");
                        }
                    }
                    if (Grid.GetColumn(element) == 1)
                    {
                        TextBox temp = (TextBox)element;
                        s += (temp.Text + "_");
                    }
                    if (Grid.GetColumn(element) == 4)
                    {
                        ToggleButton temp = (ToggleButton)element;
                        if (temp.IsChecked == true)
                            s += "1";
                        else
                            s += "0";
                    }
                }
            s = time + s;

            curlist[row] = s;
            plans[GetCur(cur)] = curlist;

            MessageBox.Show("изменение сохранено!");
        }
        
        //обработчик для кнопок удалить в каждой строке записей
        private void delete_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;
            var row = Grid.GetRow(bt);
            curlist.RemoveAt(row);
            plans[GetCur(cur)] = curlist;
            main.Children.Clear();
            main.RowDefinitions.Clear();

            ShowList(curlist);
        }

        //обработчик кнопки "Найти", выбираем дату выводим нужный нам список дел (на выбранную дату)
        private void Choose_Click(object sender, RoutedEventArgs e)
        {
            main.RowDefinitions.Clear();
            main.Children.Clear();

            if (choose.SelectedDate.Value == null)
            {
                MessageBox.Show("Выберите дату!");
                return;
            }
            cur = choose.SelectedDate.Value;
            
            try
            {
                curlist = plans[GetCur(cur)];
            }
            catch
            {
                plans.Add(GetCur(cur), new List<string>());
                curlist = plans[cur];
            }
            ShowList(plans[GetCur(cur)]);
        }

        //обработчик для кнопки "назад"
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            new_plan.Visibility = Visibility.Hidden;
            win.Visibility = Visibility.Visible;
            main.Children.Clear();
            main.RowDefinitions.Clear();
            ShowList(plans[GetCur(cur)]);
        }

        //обработчик для кнопки "добавить"
        private void New_Click(object sender, RoutedEventArgs e)
        {
            date.Content = "Для даты: " + cur.ToShortDateString();
            win.Visibility = Visibility.Hidden;
            new_plan.Visibility = Visibility.Visible;
        }

        //метод срабатывающий при закрытии окна, здесь мы сохраняем все в наш файл, т.е. сохранение в файл происходит автоматически
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(path, false))
            {
                foreach (var pair in plans)
                {
                    List<string> list = pair.Value;
                    DateTime dt = pair.Key;
                    string temp = dt.ToShortDateString();

                    foreach (string s in list)
                        temp += ("/" + s);
                    file.WriteLine(temp);

                }
            }
        }
    }
}
