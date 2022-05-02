package main
import (
	"C"
	"fmt"
	"os/exec"
	"strings"
	"syscall"
	"encoding/json"
	//"os"
	"regexp"
)

type Report struct {
    Ready bool
	Wstwo bool
	Installed string
	Online string
	Running string
}

func main() {
	generateReport()
}

func getReportToString(report *Report) string{
    b, err := json.Marshal(report)
    if err != nil {
        return "error"
    }
    return string(b)
}

//export generateReport
func generateReport() *C.char{	
	ready := checkInstalled()
	wstwoready := checkWSLTWO()
	rep := Report{Ready:ready, Wstwo:wstwoready, Installed:"", Online:"", Running:"" }
	if (ready){
		rep.Installed = runCommand("wsl -l -v")
		rep.Online = runCommand("wsl --list --online")
		rep.Running = runCommand("wsl -l --running")
	}
	//repString := fmt.Sprintf("%+v\n", rep)
	repString := getReportToString(&rep)	
	return C.CString(fmt.Sprintf("%s\n", repString))
}

//func writeToFile(input string) bool{
//	f, err := os.Create("report.txt")
//	_, err2 := f.WriteString(input)
//	if(err != nil || err2 != nil){
//		return false
//	}
//	f.Close()
//	return true
//}

//export checkInstalled
func checkInstalled() bool{
	out := runCommand("wsl --list --online")
	if strings.TrimRight(out, "\n") == "is not recognized" {
		return false
	}
	if strings.TrimRight(out, "\n") == "error" {
		return false
	}
	return true
}

//export checkWSLTWO
func checkWSLTWO() bool{
	out := runCommand("wsl --update")
	if strings.TrimRight(out, "\n") == "Downloading updates" {
		return false
	}
	if strings.TrimRight(out, "\n") == "The requested operation requires elevation" {
		return false
	}
	if strings.TrimRight(out, "\n") == "error" {
		return false
	}
	return true
}



//export runCommand
func runCommand(input string) string {
	cmd_path := "C:\\Windows\\system32\\cmd.exe"
	cmd_instance := exec.Command(cmd_path, "/c", input)
	cmd_instance.SysProcAttr = &syscall.SysProcAttr{HideWindow: true}
	cmd_output, err := cmd_instance.Output()
	if err != nil {
		return "error"
	}
	re := regexp.MustCompile(`[\x00]`)
	correct := re.ReplaceAllLiteralString(string(cmd_output), "")
	return string(correct)
}

