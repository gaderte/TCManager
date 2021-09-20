using System;
using System.Collections.ObjectModel;
using System.Windows;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;

namespace TCManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ConnectionCredentials creds;
        private TwitchClient client;
        ObservableCollection<CM> chatHisto = new ObservableCollection<CM>();
        ObservableCollection<string> subs = new ObservableCollection<string>();
        ObservableCollection<string> modo = new ObservableCollection<string>();
        CM selectedMsg = null;
        bool pauseC = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            client = new TwitchClient();
            creds = new ConnectionCredentials(channelBox.Text, tokenBox.Password);
            Console.WriteLine("Chaine pour se connecter : " + channelBox.Text + " et token : " + tokenBox.Password);
            client.Initialize(creds, channelBox.Text);
            client.OnLog += Client_onLog;
            client.OnError += Client_onError;
            client.OnMessageReceived += Client_onMessageReceived;
            client.OnDisconnected += Client_OnDisconnected;
            client.OnNewSubscriber += Client_onNewSubscriber;
            client.OnReSubscriber += Client_onReSubscriber;
            client.OnCommunitySubscription += Client_onCommunitySubscription;
            client.OnModeratorsReceived += Client_onModeratorsReceived;
            client.Connect();
            client.OnConnected += Client_onConnected;

        }

        private void Client_onModeratorsReceived(object sender, OnModeratorsReceivedArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                Console.WriteLine("Liste modérateurs reçus");
                foreach (string mods in e.Moderators)
                {
                    modo.Add(mods);
                }
            });
        }

        private void Client_onCommunitySubscription(object sender, OnCommunitySubscriptionArgs e)
        {
            Console.WriteLine("Subgift : " + e.GiftedSubscription.DisplayName);
            this.Dispatcher.Invoke(() =>
            {
                if (subs.Count >= 50)
                {
                    subs.RemoveAt(0);
                }
                subs.Add("Subgift : " + e.GiftedSubscription.DisplayName);
            });
        }

        private void Client_onReSubscriber(object sender, OnReSubscriberArgs e)
        {
            Console.WriteLine("Resub : " + e.ReSubscriber.DisplayName);
            this.Dispatcher.Invoke(() =>
            {
                if (subs.Count >= 50)
                {
                    subs.RemoveAt(0);
                }
                subs.Add("Resub : " + e.ReSubscriber.DisplayName);
            });
        }

        private void Client_onNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            Console.WriteLine("New sub : " + e.Subscriber.DisplayName);
            this.Dispatcher.Invoke(() =>
            {
                if (subs.Count >= 50)
                {
                    subs.RemoveAt(0);
                }
                subs.Add("New sub : " + e.Subscriber.DisplayName);
            });
        }

        private void Client_onConnected(object sender, OnConnectedArgs e)
        {
            Console.WriteLine("[BOT]" + e.BotUsername + " s'est connecté");
            this.Dispatcher.Invoke(() =>
            {
                ListeMessages.ItemsSource = chatHisto;
                ListeSubs.ItemsSource = subs;
                ListeModerateurs.ItemsSource = modo;
                MainWindow1.Title = "ChannelMannager - Connecté à " + client.TwitchUsername;
                ConnectButton.IsEnabled = false;
                DisconnectButton.IsEnabled = true;
                PauseTchat.IsEnabled = true;
                channelBox.IsEnabled = false;
                tokenBox.IsEnabled = false;
            });
        }

        private void Client_OnDisconnected(object sender, OnDisconnectedEventArgs e)
        {
            Console.WriteLine("Event on Disconnect : " + e.ToString());
        }

        private void Client_onMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            Console.WriteLine(e.ChatMessage.Username + " : " + e.ChatMessage.Message);
            this.Dispatcher.Invoke(() =>
            {
                if (chatHisto.Count >= 100)
                {
                    chatHisto.RemoveAt(0);
                }
                chatHisto.Add(new CM() { username = e.ChatMessage.Username, message = e.ChatMessage.Message, isSub = e.ChatMessage.IsSubscriber, isMod = e.ChatMessage.IsModerator });
            });
        }

        private void Client_onError(object sender, OnErrorEventArgs e)
        {
            Console.WriteLine("Une erreur s'est produite : " + e.ToString());
        }

        private void Client_onLog(object sender, OnLogArgs e)
        {
            Console.WriteLine(e.Data);
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            //On est déconnecté : On modifie la fenêtre et on désactive le bouton "Déconnecter"
            MainWindow1.Title = "ChannelMannager - Non connecté";
            ConnectButton.IsEnabled = true;
            DisconnectButton.IsEnabled = false;
            PauseTchat.IsEnabled = false;
            channelBox.IsEnabled = true;
            tokenBox.IsEnabled = true;
            client.Disconnect();
        }

        private void PauseTchat_Click(object sender, RoutedEventArgs e)
        {
            if(pauseC)
            {
                client.OnMessageReceived += Client_onMessageReceived;
                pauseC = false;
            } else
            {
                client.OnMessageReceived -= Client_onMessageReceived;
                pauseC = true;
            }
        }

        private void ChannelBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (channelBox.Text != "" && tokenBox.Password != "")
            {
                ConnectButton.IsEnabled = true;
            } else
            {
                ConnectButton.IsEnabled = false;
            }
        }

        private void TokenBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (channelBox.Text != "" && tokenBox.Password != "")
            {
                ConnectButton.IsEnabled = true;
            }
            else
            {
                ConnectButton.IsEnabled = false;
            }
        }

        private void toPersoText_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            double temp;
            string displayTime = "(en sec) soit ";
            if(double.TryParse(toPersoText.Text, out temp))
            {
                TimeSpan time = TimeSpan.FromSeconds(temp);
                if (time.Minutes >= 1) displayTime += time.Minutes + "m, ";
                else displayTime += "0m, ";
                if (time.Hours >= 1) displayTime += time.Hours + "h, ";
                else displayTime += "0h, ";
                if (time.Days >= 1) displayTime += time.Days + "j";
                else displayTime += "0j";

                labelIndicSec.Content = displayTime;
            }
        }
        private void BanOrTo(CM msg, int time, string motif)
        {
            if(time == 0)
            {
                //client.SendMessage(channelBox.Text, "/ban " + user + " " + motif);
                Console.WriteLine("/ban " + msg.username + " " + motif);
            } else
            {
                //client.SendMessage(channelBox.Text, "/timeout " + user + " " + motif);
                Console.WriteLine("/timeout " + msg.username + " " + time);
            }
        }

        private void banDefButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedMsg == null)
            {
                MessageBox.Show("Veuillez séléctionner un message dans la liste");
            } else
            {
                BanOrTo((CM)ListeMessages.SelectedItem, 0, motifText.Text);
            }
        }

        private void toSButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedMsg == null)
            {
                MessageBox.Show("Veuillez séléctionner un message dans la liste");
            }
            else
            {
                CM temp = (CM)ListeMessages.SelectedItem;
                BanOrTo(temp, 600, motifText.Text);
            }
        }
        private void toMButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedMsg == null)
            {
                MessageBox.Show("Veuillez séléctionner un message dans la liste");
            }
            else
            {
                CM temp = (CM)ListeMessages.SelectedItem;
                BanOrTo(temp, 86400, motifText.Text);
            }
        }

        private void toLButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedMsg == null)
            {
                MessageBox.Show("Veuillez séléctionner un message dans la liste");
            }
            else
            {
                CM temp = (CM)ListeMessages.SelectedItem;
                BanOrTo(temp, 604800, motifText.Text);
            }
        }

        private void toPersoButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedMsg == null)
            {
                MessageBox.Show("Veuillez séléctionner un message dans la liste");
            }
            else
            {
                int temp;
                if(int.TryParse(toPersoText.Text, out temp))
                {
                    CM tempM = (CM)ListeMessages.SelectedItem;
                    BanOrTo(tempM, temp, motifText.Text);
                } else
                {
                    MessageBox.Show("Le temps rentré n'est pas un chiffre/nombre");
                }
            }
        }

        private void ListeMessages_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if(ListeMessages.SelectedIndex != -1)
            {
                selectedMsg = (CM)ListeMessages.SelectedItem;
            } else
            {
                selectedMsg = null;
            }
        }

        private void emoteOnlyOKButton_Click(object sender, RoutedEventArgs e)
        {
            client.SendMessage(channelBox.Text, "/emoteonly");
            Console.WriteLine("/emoteonly");
        }

        private void emoteOnlyNOKButton_Click(object sender, RoutedEventArgs e)
        {
            client.SendMessage(channelBox.Text, "/emoteonlyoff");
            Console.WriteLine("/emoteonlyoff");
        }

        private void slowModeOnButton_Click(object sender, RoutedEventArgs e)
        {
            Window1 window = new Window1();
            if(window.ShowDialog() == true && int.TryParse(window.Answer, out int temp))
            {
                client.SendMessage(channelBox.Text, "/slow " + window.Answer);
                Console.WriteLine("Activation du mode lent à " + window.Answer);
            } else
            {
                MessageBox.Show("Problème lors de la saisie du temps");
            }
        }

        private void slowModeOffButton_Click(object sender, RoutedEventArgs e)
        {
            client.SendMessage(channelBox.Text, "/slowoff");
            Console.WriteLine("Désactivation du mode lent");
        }

        private void subOnlyButton_Click(object sender, RoutedEventArgs e)
        {
            client.SendMessage(channelBox.Text, "/subscribers");
            Console.WriteLine("Mode sub only");
        }

        private void subOnlyOff_Click(object sender, RoutedEventArgs e)
        {
            client.SendMessage(channelBox.Text, "/subscribersoff");
            Console.WriteLine("Désactivation du Mode sub only");
        }
    }
}
