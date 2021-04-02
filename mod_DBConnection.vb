Imports System.Data.Odbc
Imports System.Security.Cryptography
Imports System.Text
Imports System.Data.SQLite

' SQLite Database Connectivity

Module mod_DBConnection

    Public reader As SQLiteDataReader
    Dim con As SQLiteConnection
    Dim cmd As SQLiteCommand

    Public Sub init_connection()
        Try
            Dim cs As String = "URI=file:rsched_rooms.db"

            con = New SQLiteConnection(cs)
            con.Open()
            Dim stm As String = "SELECT SQLITE_VERSION()"
            cmd = New SQLiteCommand(stm, con)
            Dim version As String = Convert.ToString(cmd.ExecuteScalar())
            Console.WriteLine("SQLite version : {0}", version)
            con.Close()
        Catch ex As SQLiteException
            Console.WriteLine("Error: " & ex.ToString())
            con.Close()
        End Try

        'CREATE REQUIRED TABLES
        initTables()

    End Sub


    'DB Functions


    Public Sub initTables()

        Dim cmd1 As New SQLiteCommand()
        Dim cmd2 As New SQLiteCommand()
        Dim cmd3 As New SQLiteCommand()

        Try
            con.Open()
            With cmd1
                .Connection = con
                .CommandText = "CREATE TABLE IF NOT EXISTS 'room_account' ('USER_ID' TEXT NOT NULL UNIQUE , 'PASSWORD' TEXT NOT NULL , 'NAME' TEXT NOT NULL DEFAULT 'user' , PRIMARY KEY('USER_ID'));"
                .Prepare()
                .ExecuteNonQuery()
            End With
            con.Close()
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        Finally
            con.Close()
        End Try

        Try
            con.Open()
            With cmd2
                .Connection = con
                .CommandText = "CREATE TABLE IF NOT EXISTS 'room_schedules' ('SCHED_ID'	INTEGER NOT NULL , 'ROOM_ID' TEXT NOT NULL, 'FROM' NUMERIC , 'TO' NUMERIC , 'DATE_FROM' TEXT , 'DATE_TO' TEXT , 'BY_USER' TEXT , PRIMARY KEY('SCHED_ID' AUTOINCREMENT));"
                .Prepare()
                .ExecuteNonQuery()
            End With
            con.Close()
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        Finally
            con.Close()
        End Try

        Try
            con.Open()
            With cmd3
                .Connection = con
                .CommandText = "CREATE TABLE IF NOT EXISTS 'rooms' ('ROOM_ID' TEXT NOT NULL UNIQUE , 'Location' TEXT , 'Status' TEXT , PRIMARY KEY('ROOM_ID'));"
                .Prepare()
                .ExecuteNonQuery()
            End With
            con.Close()
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        Finally
            con.Close()
        End Try

    End Sub



    Public Function addRoom(ByVal ROOM_ID As String, ByVal Location As String) As String
        Dim output As String = ""
        Try
            con.Open()
            Dim cmd As New SQLiteCommand()
            With cmd
                .Connection = con
                .CommandText = "INSERT INTO rooms (`ROOM_ID`, `Location`) VALUES ('" + ROOM_ID + "', '" + Location + "');"
                .Prepare()
                .ExecuteNonQuery()
            End With
            con.Close()
            output = ROOM_ID & " Added"
        Catch ex As Exception
            output = "Room Already Exist"
        Finally
            con.Close()
        End Try
        Return output
    End Function

    Public Sub addSchedule(ByVal ROOM_ID As String, ByVal _FROM As String, ByVal _TO As String, ByVal _BY_USER As String)
        '_FROM AND _TO is Date.Ticks.ToString
        Dim __TO__ As String = New Date(Long.Parse(_TO)).ToString
        Dim __FROM__ As String = New Date(Long.Parse(_FROM)).ToString
        Try
            con.Open()
            Dim cmd As New SQLiteCommand()
            With cmd
                .Connection = con
                .CommandText = "INSERT INTO room_schedules (`ROOM_ID`, `DATE_FROM` , `DATE_TO` ,`FROM`, `TO`, `BY_USER`) VALUES ('" + ROOM_ID + "', '" + __FROM__ + "' , '" + __TO__ + "' ," + _FROM + ", " + _TO + " , '" + _BY_USER + "');"
                .Prepare()
                .ExecuteNonQuery()
            End With
            con.Close()
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        Finally
            con.Close()
        End Try
    End Sub

    Public Sub removeSchedule(ByVal SCHED_ID As Integer)
        Try
            con.Open()
            Dim cmd As New SQLiteCommand()
            With cmd
                .Connection = con
                .CommandText = "DELETE FROM room_schedules WHERE SCHED_ID=" + Str(SCHED_ID) + ";"
                .Prepare()
                .ExecuteNonQuery()
            End With
            con.Close()
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        Finally
            con.Close()
        End Try
    End Sub


    Public Function findAvailable(ByVal _FROM As Long, ByVal _TO As Long) As List(Of String)
        Dim availableRooms As New List(Of String)
        Dim timeStamps As List(Of Long)



        For Each room In getListOfRooms()
            timeStamps = getListOfTimeStamps(room)
            'Console.WriteLine(room)
            If Not timeStamps.Count <= 0 Then
                timeStamps.Sort()
                If timeStamps.Item(timeStamps.Count - 1) <= _FROM Then
                    'Console.WriteLine(2)
                    availableRooms.Add(room)
                    Continue For
                ElseIf timeStamps.Item(0) >= _TO Then
                    'Console.WriteLine(3)
                    availableRooms.Add(room)
                    Continue For
                End If

                For i As Integer = 0 To timeStamps.Count - 1 Step 1
                    If Not i >= timeStamps.Count - 1 And Not i Mod 2 = 0 Then
                        If timeStamps.Item(i) <= _FROM And timeStamps.Item(i + 1) >= _TO Then
                            'Console.WriteLine(Str(4) + "--" + Str(i))
                            availableRooms.Add(room)
                            Continue For
                        Else
                            Continue For
                        End If
                    End If
                Next

                timeStamps.Clear()

            Else
                availableRooms.Add(room)
            End If
        Next


        Return availableRooms

    End Function

    Public Function getListOfRooms() As List(Of String)
        Dim Rooms As New List(Of String)
        Try
            con.Open()
            Dim cmd As New SQLiteCommand()
            cmd.Connection = con
            cmd.CommandText = "SELECT * FROM rooms;"
            reader = cmd.ExecuteReader
            While reader.Read
                Rooms.Add(reader(0))
            End While
            reader.Close()
        Catch ex As Exception
        Finally
            con.Close()
        End Try
        Return Rooms
    End Function

    Public Function getListOfTimeStamps(ByVal ROOM_ID As String) As List(Of Long)
        Dim output As New List(Of Long)
        Dim temp As String = ""
        Try
            con.Open()
            Dim cmd As New SQLiteCommand()
            cmd.Connection = con
            cmd.CommandText = "SELECT * FROM room_schedules where ROOM_ID = '" + ROOM_ID + "';"
            reader = cmd.ExecuteReader
            While reader.Read
                temp = reader(2)
                output.Add(Long.Parse(reader(2)))
                temp = reader(3)
                output.Add(Long.Parse(reader(3)))
            End While
            reader.Close()
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        Finally
            con.Close()
        End Try
        output.Sort()
        Return output
    End Function

    Public Sub removeFinishedSchedules(ByVal dateNow As Date)
        Try
            con.Open()
            Dim cmd As New SQLiteCommand()
            With cmd
                .Connection = con
                .CommandText = "DELETE FROM room_schedules WHERE `TO` < " + dateNow.Ticks.ToString + ";"
                .Prepare()
                .ExecuteNonQuery()
            End With
            con.Close()
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        Finally
            con.Close()
        End Try
    End Sub

    Public Function getRoomsSchedTable() As DataTable
        Dim output As New DataTable
        Dim adapter As SQLiteDataAdapter
        Try
            con.Open()
            Dim cmd As New SQLiteCommand()
            cmd.Connection = con
            cmd.CommandText = "SELECT * FROM room_schedules;"
            adapter = New SQLiteDataAdapter(cmd)
            adapter.Fill(output)
            reader.Close()
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        Finally
            con.Close()
        End Try
        Return output
    End Function


    'DB ACCOUNTS

    Public Sub addAccount(ByVal USER_ID As String, ByVal PASSWORD As String, ByVal NAME As String)
        Try
            con.Open()
            Dim cmd As New SQLiteCommand()
            With cmd
                .Connection = con
                .CommandText = "INSERT INTO room_account (`USER_ID`, `PASSWORD` , `NAME`) VALUES ('" + USER_ID + "', '" + encrypted(PASSWORD) + "', '" + NAME + "');"
                .Prepare()
                .ExecuteNonQuery()
            End With
            con.Close()

        Catch ex As Exception
            Console.WriteLine(ex.Message)
        Finally
            con.Close()
        End Try
    End Sub

    Public Function authenticate(ByVal USER_ID As String, ByVal PASSWORD As String) As Boolean
        Dim passwordMatch As Boolean = False
        Try
            con.Open()
            Dim cmd As New SQLiteCommand()
            cmd.Connection = con
            cmd.CommandText = "SELECT PASSWORD FROM room_account WHERE USER_ID='" + USER_ID + "';"
            reader = cmd.ExecuteReader
            While reader.Read
                passwordMatch = encrypted(PASSWORD) = reader(0)
            End While
            reader.Close()
        Catch ex As Exception
            Console.WriteLine(ex.Message)
            Return False
        Finally
            con.Close()
        End Try
        Return passwordMatch
    End Function

    Private Function encrypted(ByVal S As String) As String
        Using hasher As MD5 = MD5.Create()
            Dim sbytes As Byte() = hasher.ComputeHash(Encoding.UTF8.GetBytes(S))
            Return Convert.ToBase64String(sbytes)
        End Using
    End Function

    Public Function validUsername(ByVal USER_ID As String) As Boolean
        Dim output As String = ""
        Try
            con.Open()
            Dim cmd As New SQLiteCommand()
            cmd.Connection = con
            cmd.CommandText = "SELECT USER_ID FROM room_account  WHERE USER_ID='" + USER_ID + "';"
            reader = cmd.ExecuteReader
            While reader.Read
                Console.WriteLine(reader(0))
                output = reader(0)
                Console.WriteLine(output.Length)
            End While
            reader.Close()
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        Finally
            con.Close()
        End Try
        Return Not output.Length > 0
    End Function


    Public Function getName(ByVal USER_ID As String) As String
        Dim output As String = ""
        Try
            con.Open()
            Dim cmd As New SQLiteCommand()
            cmd.Connection = con
            cmd.CommandText = "SELECT NAME FROM room_account WHERE USER_ID='" + USER_ID + "';"
            reader = cmd.ExecuteReader
            While reader.Read
                output = reader(0)
            End While
            reader.Close()
        Catch ex As Exception
        Finally
            con.Close()
        End Try
        Return output
    End Function









    '''''''''''''''''''''''''''''''REF'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

    'Public Function getListOfOccupiedRooms(ByVal _today As Date) As List(Of String)
    '    Dim Rooms As New List(Of String)
    '    Dim time As New List(Of Long)
    '    Try
    '        con.Open()
    '        Dim cmd As New SQLiteCommand()
    '        cmd.Connection = con
    '        cmd.CommandText = "SELECT ROOM_ID FROM roomS;"
    '        reader = cmd.ExecuteReader
    '        While reader.Read
    '            time.Clear()
    '            time = getListOfTimeStamps(reader(0).ToString)
    '            If time.Item(time.Count - 1) <= _today.Ticks Then
    '                Rooms.Add(reader(0))
    '            End If
    '        End While
    '        reader.Close()
    '    Catch ex As Exception
    '    Finally
    '        con.Close()
    '    End Try
    '    Return Rooms
    'End Function

End Module
