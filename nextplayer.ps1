$COM = "COM4"

Function SendToPipe{
    param (
    [String]$Send
    )
    try {
        $pipeClient = new-object System.IO.Pipes.NamedPipeClientStream(".", "DartOverlay", [System.IO.Pipes.PipeDirection]::Out)
        $pipeClient.Connect(2000)
        $pipeWriter = new-object System.IO.StreamWriter($pipeClient)
        $pipeWriter.AutoFlush = $true
        $pipeWriter.WriteLine($Send)
        $pipeWriter.Close()
        $pipeClient.Close()
        }
    catch{
        Write-Output "Error on SendToPipe"
    }
    finally {
            $pipeWriter.Close()
            $pipeClient.Close()
            $pipeWriter.Dispose()
            $pipeClient.Dispose()
    }
}

function read-com {
    $port= new-Object System.IO.Ports.SerialPort $COM,9600,None,8,one
    $port.Open()
    do {
        $line = $port.ReadLine()
        Write-Host $line # Do stuff here
        (get-date -Format "yyyy-MM-dd HH:mm:ss.fff") + ": " + ($line -replace "`t|`n|`r","") | out-file -Encoding Ascii -append c:\temp\nextplayer.log
        if( $line -match "NextPlayer"){
            Write-Host " -> Send {ENTER}"
			SendToPipe -Send hide
            SendWinDartEnter
        }
        if( $line -match "FoundPlayer"){
		    SendToPipe -Send show
            #$Song = New-Object System.Media.SoundPlayer
            #$Song.SoundLocation = "C:\WinDart\res_wav\dart.wav"
            #$Song.Play()
        }
		if( $line -match "bRemain1Dart: 1|bGameOn: 0"){
			SendToPipe -Send hide
        }
    }
    while ($port.IsOpen)
}


function SendWinDartEnter{
Add-Type -AssemblyName System.Windows.Forms
$sig=@'
[DllImport("user32.dll", CharSet = CharSet.Auto)]
public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
'@    

$Dlls = @' 
    [DllImport("user32.dll")] 
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")] 
    public static extern bool SetForegroundWindow(IntPtr hWnd); 
'@


$WindowControl = Add-Type -MemberDefinition $Dlls -Name "Win32WindowControl" -namespace Win32Functions -passThru
$w32 = Add-Type -Namespace Win32 -Name Funcs -MemberDefinition $sig -PassThru
$WindowHandle = $w32::FindWindow('TfrmCricket', '') # Windows PowerShell Console
If($WindowHandle){
            $WindowControl::SetForegroundWindow($WindowHandle)
            Start-Sleep -Milliseconds 10
            [System.Windows.Forms.SendKeys]::SendWait("{ENTER}")
}
}

Start-Process C:\Daten\WinDart\WindartOverlay.exe -ArgumentList "/hide /opacity 40"
read-com