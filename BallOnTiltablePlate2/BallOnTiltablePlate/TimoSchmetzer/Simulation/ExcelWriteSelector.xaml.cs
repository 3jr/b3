using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Reflection;

namespace BallOnTiltablePlate.TimoSchmetzer.Simulation
{
    /// <summary>
    /// Interaction logic for ExcelWriteSelector.xaml
    /// </summary>
    public partial class ExcelWriteSelector : Window
    {
        public ExcelWriteSelector()
        {
            InitializeComponent();
            IEnumerable<Type> Calculators = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsClass && (typeof(IControledSystemModule).IsAssignableFrom(t) || typeof(IPhysicsCalculator).IsAssignableFrom(t)))
            .OrderBy(t => t.FullName)
            .Select(t => t)
            .ToArray();
            foreach (Type t in Calculators)
            {
                TreeViewItem treeitem = new TreeViewItem();
                treeitem.Header = t.FullName;
                treeitem.Tag = t;
                TypeList.Items.Add(treeitem);
            }
        }

        public Type SelectedType = null;

        private void OKExecuted_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SelectedType = (Type)(((TreeViewItem)TypeList.SelectedItem).Tag);
            TypeChosen(this, EventArgs.Empty);
            this.Close();
        }

        public event EventHandler TypeChosen;
    }
}
