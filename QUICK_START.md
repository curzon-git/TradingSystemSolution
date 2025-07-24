# 🚀 Quick Start Guide

## ⚡ **Get Running in 2 Minutes**

### **Step 1: Build the Solution**

**Windows:**
```cmd
build.bat
```

**Linux/Mac:**
```bash
./build.sh
```

**Manual:**
```bash
dotnet restore
dotnet build
```

### **Step 2: Start the Trading Interface**
```bash
cd TradingWebInterface
dotnet run
```

**You'll see:**
```
=== C# Trading Web Interface Starting ===
Local access: http://localhost:5000
Remote access: http://[your-server-ip]:5000
Press Ctrl+C to stop the server
```

### **Step 3: Open in Browser**
- Go to: **http://localhost:5000**
- You'll see the trading interface with live data!

### **Step 4: Test Your APIs (Optional)**
```bash
# In a new terminal
cd ClientExample
dotnet run
```

## 🎯 **What You'll See**

### **Web Interface**
- 📝 **Order Entry Panel** - Symbol, quantity, price inputs
- 📊 **Live Positions** - Real-time P&L updates  
- 🖥️ **Screen Fields** - All GetGUIField/PutGUIField accessible fields

### **API Demo Output**
```
=== C# Trading Interface Client Demo ===

1. Testing your requested APIs:
   GetGUIField('webpagetagname', 'symbol_input') = ''
   PutGUIField('webpagetagname', 'symbol_input', 'AAPL') = True
   Verification - GetGUIField('webpagetagname', 'symbol_input') = 'AAPL'

2. Setting up a complete order:
   Order fields set:
   Symbol: MSFT
   Quantity: 100
   Price: 380.50
   Type: BUY

3. Testing button callback:
   Place order callback result: True
   Status after order: 'Order executed: BUY 100 MSFT @ $380.50'

✅ Your requested APIs are working perfectly!
```

## 🌐 **Remote Access**

### **From Another Computer**
1. Find your server's IP address
2. Open: `http://[server-ip]:5000`
3. Multiple users can access simultaneously!

### **Test Remote APIs**
```csharp
// Change the URL in ClientExample/Program.cs
var client = new TradingInterfaceClient("http://your-server-ip:5000");
```

## 🔧 **Integration with Your Trading System**

### **Replace Mock Data**
Edit `TradingWebInterface/Services/TradingSystemService.cs`:

```csharp
public async Task<AccountSummary> GetAccountSummaryAsync()
{
    // Replace this line:
    // Mock implementation...
    
    // With your trading system:
    return await YourTradingSystem.GetAccountSummaryAsync();
}
```

## ✅ **Success Checklist**

- [ ] Solution builds without errors
- [ ] Web interface loads at http://localhost:5000
- [ ] Can enter order details in the form
- [ ] Place Order button works and shows confirmation
- [ ] Positions table shows live data
- [ ] Screen Fields section shows all field values
- [ ] Client example runs and demonstrates APIs
- [ ] Can access from another computer (optional)

## 🆘 **Troubleshooting**

### **Build Errors**
```bash
# Clear and rebuild
dotnet clean
dotnet restore
dotnet build
```

### **Port Already in Use**
```bash
# Check what's using port 5000
netstat -tulpn | grep :5000

# Kill the process or use different port
dotnet run --urls "http://localhost:5001"
```

### **Can't Access Remotely**
```bash
# Make sure it's listening on all interfaces
dotnet run --urls "http://0.0.0.0:5000"

# Check firewall (Windows)
# Allow port 5000 in Windows Firewall

# Check firewall (Linux)
sudo ufw allow 5000
```

## 🎉 **You're Ready!**

Your C# Trading Interface is now running with:
- ✅ **GetGUIField/PutGUIField APIs** working
- ✅ **Button callbacks** handling user actions
- ✅ **Multi-machine access** capability
- ✅ **Real-time data** updates
- ✅ **Professional interface** ready for trading

**Next:** Integrate with your actual trading system by replacing the mock data!

