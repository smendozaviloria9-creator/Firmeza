using Firmeza.Web.Utils;
using Xunit;

namespace Firmeza.Tests;

public class EdadValidatorTests
{
    [Theory]
    [InlineData("18", 18)]
    [InlineData("25", 25)]
    [InlineData("60", 60)]
    [InlineData("120", 120)]
    [InlineData("  35  ", 35)]
    public void TryParseEdad_ConEdadValida_RetornaTrueYEdadCorrecta(string input, int esperado)
    {
        // Act
        var resultado = EdadValidator.TryParseEdad(input, out var edad, out var mensajeError);

        // Assert
        Assert.True(resultado);
        Assert.Equal(esperado, edad);
        Assert.Null(mensajeError);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("17")]
    [InlineData("-5")]
    public void TryParseEdad_ConEdadMenorA18_RetornaFalse(string input)
    {
        // Act
        var resultado = EdadValidator.TryParseEdad(input, out var edad, out var mensajeError);

        // Assert
        Assert.False(resultado);
        Assert.Equal(0, edad);
        Assert.NotNull(mensajeError);
        Assert.Contains("entre 18 y 120", mensajeError);
    }

    [Theory]
    [InlineData("121")]
    [InlineData("200")]
    public void TryParseEdad_ConEdadMayorA120_RetornaFalse(string input)
    {
        // Act
        var resultado = EdadValidator.TryParseEdad(input, out var edad, out var mensajeError);

        // Assert
        Assert.False(resultado);
        Assert.Equal(0, edad);
        Assert.NotNull(mensajeError);
        Assert.Contains("entre 18 y 120", mensajeError);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryParseEdad_ConTextoNuloOVacio_RetornaFalse(string? input)
    {
        // Act
        var resultado = EdadValidator.TryParseEdad(input, out var edad, out var mensajeError);

        // Assert
        Assert.False(resultado);
        Assert.NotNull(mensajeError);
        Assert.Equal("Debes ingresar la edad del cliente.", mensajeError);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("25a")]
    [InlineData("veinticinco")]
    [InlineData("18.5")]
    public void TryParseEdad_ConTextoNoNumerico_RetornaFalse(string input)
    {
        // Act
        var resultado = EdadValidator.TryParseEdad(input, out var edad, out var mensajeError);

        // Assert
        Assert.False(resultado);
        Assert.NotNull(mensajeError);
        Assert.Contains("no es un número entero válido", mensajeError);
    }

    [Fact]
    public void TryParseEdad_ConOverflow_RetornaFalse()
    {
        // Act
        var resultado = EdadValidator.TryParseEdad("9999999999999999999999999", out var edad, out var mensajeError);

        // Assert
        Assert.False(resultado);
        Assert.NotNull(mensajeError);
        Assert.Contains("demasiado grande", mensajeError);
    }
}
