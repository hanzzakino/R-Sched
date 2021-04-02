Module mod_Date
    Public Function monthToString(ByVal n As Integer) As String
        Dim months() As String = {"January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"}
        Return months(n - 1)
    End Function

    'Public Function toDbDate(ByVal _date As Date) As String
    '    'Dim dateString As String = _date.ToString("yyyy-mm-dd HH:mm:ss")
    '    Dim output As String = ""
    '    ' DB Date Format    2021-03-26 09:56:22 - (YYYY-MM-DD HH:MM:SS) 24HOUR FORMAT
    '    ' VB.NET Date Format    [3/26/2021 9:56:22 PM]

    '    output = _date.Year.ToString + "-" + _date.Month.ToString + "-" + _date.Day.ToString + " " + _date.ToString("HH:mm:ss")

    '    Return output
    'End Function

End Module
