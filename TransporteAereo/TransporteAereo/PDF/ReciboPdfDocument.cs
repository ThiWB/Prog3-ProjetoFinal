using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TransporteAereo.Models; 
using System;
using System.Linq;
using System.Collections.Generic;

namespace TransporteAereo.PDF
{
    public class ReciboPdfDocument : IDocument
    {
        public Compra Model { get; }

        public ReciboPdfDocument(Compra model)
        {
            Model = model;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(35);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Helvetica"));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().Element(ComposeFooter);
                });
        }

        void ComposeHeader(IContainer container)
        {
            container.PaddingBottom(15).Column(column =>
            {
                column.Item().Text("TRANSPORTES DOS GURI | RECIBO")
                    .FontSize(14).Bold().AlignCenter();
                column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
            });
        }

        void ComposeContent(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().PaddingBottom(10).Text(x =>
                {
                    x.Span("Transação ID: ").SemiBold();
                    string idPart = Model.Id.ToString().Length >= 8 ? Model.Id.ToString().Substring(0, 8).ToUpper() : Model.Id.ToString().ToUpper();
                    x.Span(idPart);
                    x.Span(" | Data: ").SemiBold();
                    x.Span($"{Model.DataCompra:dd/MM/yyyy HH:mm}");
                });

                column.Item().PaddingTop(5).Text("DETALHES DA VIAGEM").FontSize(11).Bold();

                column.Item().Row(row =>
                {
                    row.RelativeColumn(1).Text($"Viagem: {Model.Viagem.NomeViagem}").SemiBold();
                    row.RelativeColumn(1).Text($"Origem: {Model.Viagem.AeroportoOrigem.CidadeAeroporto} ({Model.Viagem.AeroportoOrigem.NomeAeroporto})");
                    row.RelativeColumn(1).Text($"Destino: {Model.Viagem.AeroportoDestino.CidadeAeroporto} ({Model.Viagem.AeroportoDestino.NomeAeroporto})");
                });

                column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                column.Item().PaddingBottom(5).Text("ASSENTOS CONFIRMADOS").FontSize(11).Bold();

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(4);
                        columns.ConstantColumn(80);
                        columns.ConstantColumn(80);
                    });

                    table.Header(header =>
                    {
                        header.Cell().BorderBottom(1).PaddingBottom(4).Text("Trecho e Horário").Bold();
                        header.Cell().BorderBottom(1).PaddingBottom(4).Text("Assento").Bold();
                        header.Cell().BorderBottom(1).PaddingBottom(4).Text("Preço").Bold().AlignRight();
                    });

                    foreach (var reserva in Model.Reservas.OrderBy(r => r.VooPoltrona.Voo.HorarioSaida))
                    {
                        var voo = reserva.VooPoltrona.Voo;
                        var assento = reserva.VooPoltrona.Assento;

                        table.Cell().PaddingVertical(4).Text($"{voo.AeroportoOrigem.NomeAeroporto} → {voo.AeroportoDestino.NomeAeroporto}");
                        table.Cell().PaddingVertical(4).Text(assento.NumeroAssento);
                        table.Cell().PaddingVertical(4).Text(reserva.PrecoReserva.ToString("C")).AlignRight().SemiBold();
                    }
                });


                column.Item().PaddingTop(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                column.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeColumn().Text(""); 

                    row.ConstantColumn(150).AlignRight().Text(x =>
                    {
                        x.Span("TOTAL PAGO: ").FontSize(12).Bold();
                        x.Span($"{Model.PrecoTotal:C}").FontSize(12).Bold().FontColor(Colors.Green.Darken2);
                    });
                });

                column.Item().PaddingTop(20).Text("NOTA: Apresentação obrigatória de documento de identificação no check-in.").FontSize(9).Italic();
            });
        }

        void ComposeFooter(IContainer container)
        {
            container.PaddingTop(10)
                .BorderTop(1)
                .BorderColor(Colors.Grey.Lighten2)
                .AlignRight()
                .Text(x =>
                {
                    x.Span("Página ").FontSize(8);
                    x.CurrentPageNumber().FontSize(8);
                    x.Span(" de ").FontSize(8);
                    x.TotalPages().FontSize(8);
                });
        }
    }
}