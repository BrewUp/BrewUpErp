using BrewSpa.MasterData.Application.Models;
using Microsoft.AspNetCore.Components;

namespace BrewSpa.MasterData.Facade.Components.Customers;

public partial class AddCustomer : ComponentBase, IDisposable
{
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public CreateCustomerJson Customer { get; set; } = new();
    [Parameter] public EventCallback<CreateCustomerJson> OnSubmit { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }

    private CreateCustomerJson _customer = new();
    private bool _isVisible;

    protected override void OnParametersSet()
    {
        _isVisible = IsVisible;
        _customer = new CreateCustomerJson
            {
                RagioneSociale = Customer.RagioneSociale,
                PartitaIva = Customer.PartitaIva,
                Indirizzo = new IndirizzoJson
                {
                    Via = Customer.Indirizzo.Via,
                    NumeroCivico = Customer.Indirizzo.NumeroCivico,
                    Citta = Customer.Indirizzo.Citta,
                    Provincia = Customer.Indirizzo.Provincia,
                    Cap = Customer.Indirizzo.Cap,
                    Nazione = Customer.Indirizzo.Nazione
                }
            };
    }

    private async Task Submit()
    {
        await OnSubmit.InvokeAsync(_customer);
    }

    private async Task Cancel()
    {
        await OnCancel.InvokeAsync();
    }
    
    #region Dispose
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Cleanup resources if needed
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~AddCustomer()
    {
        Dispose(false);
    }
    #endregion
}