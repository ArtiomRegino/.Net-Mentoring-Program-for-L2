using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace task_2._3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly ObservableCollection<Product> _resourses;
        private readonly ObservableCollection<Product> _cart;
        private CancellationTokenSource _source;
        private double _result;

        public MainWindow()
        {
            _resourses = new ObservableCollection<Product>();
            _cart = new ObservableCollection<Product>();
            FulfilStore(_resourses);
            InitializeComponent();
            AssortListView.ItemsSource = _resourses;
            CartListView.ItemsSource = _cart;
        }

        private void FulfilStore(ICollection<Product> resourses)
        {
            resourses.Add(new Product("Молоко", 6.75));
            resourses.Add(new Product("Батон", 3.20));
            resourses.Add(new Product("Горох", 5.64));
            resourses.Add(new Product("Сёмга", 10.95));
            resourses.Add(new Product("Баклажан", 4.66));
            resourses.Add(new Product("Морс", 8.11));
        }

        private async void AddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            await AddToCartAsync(AssortListView.SelectedItems);
        }

        private Task AddToCartAsync(IEnumerable products)
        {
            return Task.Run(() =>
            {
                foreach (var item in products)
                {
                    var product = (Product)item;

                    var currentProduct = _cart.FirstOrDefault(prod => prod.Name == product.Name
                                                                      && Math.Abs(prod.Price - product.Price) < 0.01);
                    if (currentProduct != null)
                    {
                        currentProduct.Quantity = currentProduct.Quantity + 1;
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _cart.Add(new Product(product.Name, product.Price, 1));
                        });
                    }
                    CountResult(product.Price);
                }
            });
        }

        private async Task RemoveFromCartButton_Click(object sender, RoutedEventArgs e)
        {
            await RemoveFromCartAsync(CartListView.SelectedItems);
        }

        private Task RemoveFromCartAsync(IList products)
        {
            return Task.Run(() =>
            {
                List<Product> cache = new List<Product>();

                foreach (var item in products)
                {
                    var product = (Product)item;

                    var currentProduct = _cart.FirstOrDefault(prod => prod.Name == product.Name
                                                                      && Math.Abs(prod.Price - product.Price) < 0.01);
                    if (currentProduct != null && currentProduct.Quantity > 1)
                    {
                        currentProduct.Quantity = currentProduct.Quantity - 1;
                    }
                    else
                    {
                        cache.Add(currentProduct);
                    }

                    CountResult(-product.Price);
                }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    cache.ForEach(prod => { _cart.Remove(prod); });
                });
            });
        }

        private void CountResult(double price)
        {
            _result += price;
            Application.Current.Dispatcher.Invoke(() =>
            {
                ResultTextBox.Clear();
                ResultTextBox.Text = _result.ToString(CultureInfo.InvariantCulture);
            });
        }
    }
}
