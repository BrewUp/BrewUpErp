using BrewUp.MasterData.SharedKernel.Dtos;
using BrewUp.Shared.CustomTypes;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ExternalContracts.MasterData;
using BrewUp.Shared.Helpers;
using BrewUp.Shared.Messages.Events;

namespace BrewUp.MasterData.Domain.Helpers;

public static class ValueObjectHelper
{
    public static IndirizzoJson ToIndirizzoJson(this Indirizzo indirizzo) =>
        new()
        {
            Via = indirizzo.Via.Value,
            Citta = indirizzo.Citta.Value,
            Cap = indirizzo.Cap.Value,
            Provincia = indirizzo.Provincia.Value,
            Nazione = indirizzo.Nazione.Value
        };

    private static Indirizzo ToIndirizzo(this IndirizzoJson indirizzoJson) =>
        new(new Via(indirizzoJson.Via),
            new NumeroCivico(indirizzoJson.NumeroCivico),
            new Cap(indirizzoJson.Cap),
            new Citta(indirizzoJson.Citta),
            new Provincia(indirizzoJson.Provincia),
            new Nazione(indirizzoJson.Nazione));

    public static CustomerCreated ToCustomerCreated(this Customer customer) => 
        new (new CustomerId(customer.Id), new RagioneSociale(customer.RagioneSociale),
        new PartitaIva(customer.PartitaIva),
        BeerConsumerLevel.FromName(customer.ConsumerLevel),
        customer.Indirizzo.ToIndirizzo());
}