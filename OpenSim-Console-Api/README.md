# OpenSim Console API

Secure Web-Based Console Interface for OpenSimulator

Version: 1.0.0 | License: BSD 3-Clause | Platform: .NET 8.0

## 📋 Overview

OpenSim Console API provides a secure, web-based interface for executing OpenSimulator console commands remotely. This tool enables administrators to manage their OpenSim grid through a modern web interface with enterprise-grade security features.

### ✨ Key Features

- **🔐 Secure Authentication**: Session-based login with hashed passwords
- **🛡️ Security Hardening**: Command injection protection, Rate limiting, Session management
- **🖥️ Interactive Console**: Real-time command execution with history and autocomplete
- **👥 Role-Based Access**: Administrator, Operator, Moderator, Viewer roles
- **📊 Command Management**: 250+ OpenSim commands with granular control
- **📱 Responsive Design**: Works on all devices

## 🚀 Quick Start

1. **Compile**: `.\runprebuild.bat && .\compile.bat`
2. **Configure**: Edit `bin/consoleapi.json`
3. **Start**: `cd bin && dotnet OpenSim.Console.Api.dll`
4. **Access**: <http://localhost:8080>

**Default Login**: admin / admin123

See full documentation in this file below.
