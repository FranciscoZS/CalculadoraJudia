using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Data;
using System.Runtime.ConstrainedExecution;

namespace CalculadoraJudia;

public partial class MainPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _calcDisplay = string.Empty;

    [ObservableProperty]
    private string _result = string.Empty;

    [ObservableProperty]
    private int _cursorPosition;

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
        else if (int.TryParse(buttonText, out var _) || buttonText == "%" || buttonText == ".")
        {
            var ch = buttonText[0];
            CalcDisplay = CalcDisplay.Insert(CursorPosition, ch.ToString());
            CursorPosition = curPos + 1;

            if (!double.TryParse(CalcDisplay, out var _))
            {
                try
                {
                    double result = Convert.ToDouble(new DataTable().Compute(GenerateExpression(), null));
                    Result = result.ToString();
                }
                catch
                {
                    // swallow
                }
            }
        }
        else if (buttonText == "=")
        {
            try
            {
                double result = Convert.ToDouble(new DataTable().Compute(GenerateExpression(), null));
                Result = result.ToString();
            }
            catch
            {
                Result = "Format error";
                return;
            }

            CalcDisplay = Result;
            Result = string.Empty;
        }
        else if (buttonText == "DEL")
        {
            if (!string.IsNullOrEmpty(CalcDisplay))
                CalcDisplay = CalcDisplay.Remove(CalcDisplay.Length - 1);
        }
        else
        {
            var ch = buttonText[0];
            CalcDisplay = CalcDisplay.Insert(CursorPosition, ch.ToString());
            CursorPosition = curPos + 1;
        }
    }

    private string GenerateExpression() => CalcDisplay.Replace('×', '*').Replace('÷', '/').Replace("%", "*0.01").Replace('(', '*').Replace(")*", "*").Replace(")", "*");
}
