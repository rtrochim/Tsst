for /f "tokens=5" %%a in ('netstat -ano ^| findstr FIN_WAIT_2') do (taskkill /f /pid %%a)