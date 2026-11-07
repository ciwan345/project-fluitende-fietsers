using System;
using System.Globalization;

namespace DeFluitendeFietser.Helpers
{
    // Deze klasse rekent een eenvoudige wiskundige invoer uit.
    // Voorbeeld: (10 + 5) * 2
    public class CalculatorParser
    {
        private string input;
        private int position;

        public CalculatorParser(string expression)
        {
            input = expression.Replace(" ", "");
            input = input.Replace(',', '.');
            position = 0;
        }

        public decimal Parse()
        {
            decimal answer = ReadExpression();

            if (position < input.Length)
            {
                throw new FormatException();
            }

            return answer;
        }

        // Eerst optellen en aftrekken.
        private decimal ReadExpression()
        {
            decimal answer = ReadTerm();

            while (position < input.Length)
            {
                if (input[position] == '+')
                {
                    position++;
                    answer = answer + ReadTerm();
                }
                else if (input[position] == '-')
                {
                    position++;
                    answer = answer - ReadTerm();
                }
                else
                {
                    break;
                }
            }

            return answer;
        }

        // Vermenigvuldigen en delen hebben voorrang op optellen.
        private decimal ReadTerm()
        {
            decimal answer = ReadFactor();

            while (position < input.Length)
            {
                if (input[position] == '*')
                {
                    position++;
                    answer = answer * ReadFactor();
                }
                else if (input[position] == '/')
                {
                    position++;
                    decimal nextNumber = ReadFactor();

                    if (nextNumber == 0)
                    {
                        throw new DivideByZeroException();
                    }

                    answer = answer / nextNumber;
                }
                else
                {
                    break;
                }
            }

            return answer;
        }

        // Leest een getal, een negatief getal of een berekening tussen haakjes.
        private decimal ReadFactor()
        {
            if (position < input.Length && input[position] == '-')
            {
                position++;
                return -ReadFactor();
            }

            if (position < input.Length && input[position] == '+')
            {
                position++;
                return ReadFactor();
            }

            decimal number;

            if (position < input.Length && input[position] == '(')
            {
                position++;
                number = ReadExpression();

                if (position >= input.Length || input[position] != ')')
                {
                    throw new FormatException();
                }

                position++;
            }
            else
            {
                int start = position;

                while (position < input.Length &&
                       (char.IsDigit(input[position]) || input[position] == '.'))
                {
                    position++;
                }

                string numberText = input.Substring(start, position - start);

                if (!decimal.TryParse(numberText, NumberStyles.Number,
                    CultureInfo.InvariantCulture, out number))
                {
                    throw new FormatException();
                }
            }

            // Percentage: 25% wordt 0,25.
            if (position < input.Length && input[position] == '%')
            {
                number = number / 100;
                position++;
            }

            return number;
        }
    }
}
