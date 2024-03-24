Imports System.Globalization
Imports Microsoft.VisualBasic

Public Class ClsTools

    Public Shared Function formatIznos(ByVal input As String) As Double?

        'Dim input As String = " 99 9.000,00 "
        ' This way you can remove unwanted characters (anything that is not a digit, and the following symbols: ".", "-", ",")
        Dim fixedInput As String = Regex.Replace(input, "[^\d-,\.]", "")
        ' fixedInput now is "999.000,00"

        Dim indexOfDot As Integer = fixedInput.IndexOf(".")
        Dim indexOfComma As Integer = fixedInput.IndexOf(",")
        Dim cultureTestOrder As List(Of CultureInfo) = New List(Of CultureInfo)

        Dim parsingResult As Double?
        Try
            If indexOfDot > 0 And indexOfComma > 0 Then
                ' There are both the dot and the comma..let's check their order
                If indexOfDot > indexOfComma Then
                    ' The dot comes after the comma. It should be en-US like Culture
                    parsingResult = Double.Parse(fixedInput, NumberStyles.Number, CultureInfo.GetCultureInfo("en-US"))
                Else
                    ' The dot comes after the comma. It should be it-IT like Culture
                    parsingResult = Double.Parse(fixedInput, NumberStyles.Number, CultureInfo.GetCultureInfo("it-IT"))
                End If
            ElseIf indexOfDot = -1 And indexOfComma > 0 Then
                ' The dot comes after the comma. It should be it-IT like Culture
                parsingResult = Double.Parse(fixedInput, NumberStyles.Number, CultureInfo.GetCultureInfo("it-IT"))
            ElseIf indexOfDot = fixedInput.Length - 3 Then
                ' There is only the dot! And it is followed by exactly two digits..it should be en-US like Culture
                parsingResult = Double.Parse(fixedInput, NumberStyles.Number, CultureInfo.GetCultureInfo("en-US"))
            ElseIf indexOfComma = fixedInput.Length - 3 Then
                ' There is only the comma! And it is followed by exactly two digits..it should be en-US like Culture
                parsingResult = Double.Parse(fixedInput, NumberStyles.Number, CultureInfo.GetCultureInfo("it-IT"))
            End If
        Catch
        End Try
        If Not parsingResult.HasValue Then
            Try
                ' There is no dot or comma, or the parsing failed for some reason. Let's try a less specific parsing.
                parsingResult = Double.Parse(fixedInput, NumberStyles.Any, NumberFormatInfo.InvariantInfo)
            Catch
            End Try
        End If

        If Not parsingResult.HasValue Then
            ' Conversion not possible, throw exception or do something else
            Return 0
        Else
            ' Use parsingResult.Value
            Return parsingResult.Value
        End If
    End Function
End Class
