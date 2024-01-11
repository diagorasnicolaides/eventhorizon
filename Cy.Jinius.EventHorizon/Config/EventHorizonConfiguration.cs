using System.ComponentModel.DataAnnotations;

namespace Cy.Jinius.EventHorizon.Config;
public record EventHorizonConfiguration([Required] string WriteSchema, [Required] string ReadSchema, [Required] string WriteConnectionString, [Required] string ReadConnectionString)
{
}

