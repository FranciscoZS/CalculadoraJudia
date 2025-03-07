using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Data;

namespace CalculadoraJudia;

public partial class MainPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _calcDisplay = string.Empty;

    [ObservableProperty]
    private string _result = string.Empty;

    [ObservableProperty]
    private int _cursorPosition;

    private double _memoryValue = 0;

    [RelayCommand]
    public void HandleButtonPress(string buttonText)
    {
        var curPos = CursorPosition;

        if (buttonText == "( )")
        {
            buttonText = CalcDisplay.ToCharArray().Where(x => x == '(' || x == ')')
                .Count() % 2 == 0 ? "(" : ")";
        }

        if (buttonText == "AC")
        {
            CalcDisplay = string.Empty;
            Result = string.Empty;
            CursorPosition = 0;
        }
        else if (buttonText == "DEL")
        {
            if (!string.IsNullOrEmpty(CalcDisplay) && CursorPosition > 0)
            {
                // Eliminar el carácter en la posición anterior al cursor
                CalcDisplay = CalcDisplay.Remove(CursorPosition - 1, 1);
                CursorPosition--; // Retroceder la posición del cursor
            }
        }
        else if (buttonText == "=")
        {
            try
            {
                double result = EvaluateExpression(GenerateExpression());
                Result = result.ToString();
                _memoryValue = result; // Guardar el resultado en la memoria
            }
            catch
            {
                Result = "Format error";
                return;
            }

            CalcDisplay = Result;
            Result = string.Empty;
            CursorPosition = CalcDisplay.Length; // Mover el cursor al final
        }
        else if (buttonText == "1/x")
        {
            if (double.TryParse(CalcDisplay, out double value))
            {
                if (value != 0)
                {
                    Result = (1 / value).ToString();
                }
                else
                {
                    Result = "Error";
                }
            }
        }
        else if (buttonText == "x^y")
        {
            CalcDisplay += "^";
            CursorPosition = curPos + 1;
        }
        else if (buttonText == "√")
        {
            if (double.TryParse(CalcDisplay, out double value))
            {
                if (value >= 0)
                {
                    Result = Math.Sqrt(value).ToString();
                }
                else
                {
                    Result = "Error";
                }
            }
        }
        else if (buttonText == "M")
        {
            if (double.TryParse(CalcDisplay, out double value))
            {
                _memoryValue = value; // Guardar el valor actual en la memoria
            }
        }
        else if (buttonText == "Ans")
        {
            // Insertar el valor de la memoria en la pantalla
            CalcDisplay = _memoryValue.ToString();
            CursorPosition = CalcDisplay.Length;
        }
        else
        {
            var ch = buttonText[0];
            CalcDisplay = CalcDisplay.Insert(CursorPosition, ch.ToString());
            CursorPosition = curPos + 1;

            if (!double.TryParse(CalcDisplay, out var _))
            {
                try
                {
                    double result = EvaluateExpression(GenerateExpression());
                    Result = result.ToString();
                }
                catch
                {
                    // swallow
                }
            }
        }
    }

    private string GenerateExpression()
    {
        return CalcDisplay.Replace('×', '*')
                          .Replace('÷', '/')
                          .Replace("%", "*0.01")
                          .Replace('(', '*')
                          .Replace(")*", "*")
                          .Replace(")", "*");
    }

    private double EvaluateExpression(string expression)
    {
        // Manejar la potencia (x^y) antes de pasar la expresión al DataTable.Compute
        if (expression.Contains("^"))
        {
            var parts = expression.Split('^');
            if (parts.Length == 2 && double.TryParse(parts[0], out double baseValue) && double.TryParse(parts[1], out double exponent))
            {
                return Math.Pow(baseValue, exponent);
            }
            else
            {
                throw new InvalidOperationException("Invalid power expression.");
            }
        }

        // Evaluar otras expresiones usando DataTable.Compute
        return Convert.ToDouble(new DataTable().Compute(expression, null));
    }
}