using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ZeBusRoute.Models;

namespace ZeBusRoute.ViewModels;

public class TicketsViewModel : INotifyPropertyChanged
{
    private int _selectedTabIndex;
    
    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<TicketType> AvailableTickets { get; set; }
    public ObservableCollection<UserTicket> MyTickets { get; set; }

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            if (_selectedTabIndex != value)
            {
                _selectedTabIndex = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand PurchaseTicketCommand { get; }

    public TicketsViewModel()
    {
        AvailableTickets = new ObservableCollection<TicketType>
        {
            new TicketType 
            { 
                Name = "Single Ride", 
                Description = "Valid for 90 minutes", 
                Price = 2.00m 
            },
            new TicketType 
            { 
                Name = "Day Pass", 
                Description = "Unlimited rides for 24 hours", 
                Price = 8.00m 
            },
            new TicketType 
            { 
                Name = "Monthly Pass", 
                Description = "Unlimited rides for 30 days", 
                Price = 50.00m 
            }
        };

        MyTickets = new ObservableCollection<UserTicket>
        {
            new UserTicket 
            { 
                Id = "T001", 
                Type = "Monthly Pass", 
                ValidUntil = new DateTime(2026, 2, 5),
                QrCodeData = "TICKET_T001_MONTHLY"
            },
            new UserTicket 
            { 
                Id = "T002", 
                Type = "Day Pass", 
                ValidUntil = new DateTime(2026, 1, 6),
                QrCodeData = "TICKET_T002_DAY"
            }
        };

        PurchaseTicketCommand = new Command<TicketType>(OnPurchaseTicket);
    }

    private async void OnPurchaseTicket(TicketType? ticket)
    {
        if (ticket == null) return;
        
        // TODO: Implement actual purchase logic
        if (Application.Current?.Windows.Count > 0)
        {
            var mainPage = Application.Current.Windows[0].Page;
            await mainPage.DisplayAlertAsync(
                "Purchase", 
                $"Purchasing {ticket.Name} for {ticket.PriceDisplay}", 
                "OK");
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}