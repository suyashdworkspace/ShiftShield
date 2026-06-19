# ShiftShield 🛡️

> **Quantitative Evaluation of Shift-Left DevSecOps** — A controlled experimental platform measuring the impact of early container vulnerability scanning on CI/CD performance, security posture, and development efficiency.

[![Pipeline A](https://img.shields.io/badge/Pipeline%20A-No%20Scan-red?style=flat-square&logo=github-actions)](https://github.com)
[![Pipeline B](https://img.shields.io/badge/Pipeline%20B-Shift--Left-green?style=flat-square&logo=github-actions)](https://github.com)
[![Pipeline C](https://img.shields.io/badge/Pipeline%20C-Shift--Right-orange?style=flat-square&logo=github-actions)](https://github.com)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com)
[![Docker](https://img.shields.io/badge/Docker-Compose-blue?style=flat-square&logo=docker)](https://docker.com)
[![Azure](https://img.shields.io/badge/Azure-Container%20Apps-blue?style=flat-square&logo=microsoft-azure)](https://azure.microsoft.com)
[![Trivy](https://img.shields.io/badge/Trivy-v0.48+-blue?style=flat-square)](https://trivy.dev)

---

## 📋 Research Overview

**Author:** Suyash Maruti Desai (Roll: 23B15)
**Institution:** SIES College of Management Studies, Navi Mumbai — Department of Computer Applications
**Course:** MCARP42 — Research Paper, MCA Semester IV
**Guide:** Prof. Vidya Rao

### Research Questions
| ID | Question |
|----|----------|
| RQ1 | Does build-stage scanning significantly reduce CVE escape rate? |
| RQ2 | What pipeline overhead does shift-left scanning add? |
| RQ3 | Does early detection reduce Mean Time to Remediate (MTTR)? |
| RQ4 | How many CVEs does Trivy detect per run across the injected set? |

### Key Results (180 Pipeline Runs)
| Metric | Pipeline A | Pipeline B | Pipeline C |
|--------|------------|------------|------------|
| CVE Escape Rate | 100% | **0%** | 100% (registry) |
| Mean Duration (Local) | 58.10s | 76.10s (+31%) | 76.90s (+32%) |
| Mean Duration (Azure) | 71.10s | 75.43s (+6.1%) | 87.40s (+23%) |
| CVEs Detected | 0 | **6** | 6 (post-push) |
| MTTR | N/A | **15 min** | 75 min |

---

## 🏗️ Architecture

ShiftShield comprises five .NET 8.0 HTTP API microservices:

```
Internet
    │
    ▼
┌─────────────┐
│  API Gateway │  ← Ocelot Reverse Proxy
│ (port 5000) │
└──────┬──────┘
       │ routes to
   ┌───┴────────────────────────────────┐
   │                                    │
   ▼                                    ▼
┌──────────────┐  ┌────────────────┐  ┌──────────────────┐
│IdentityService│  │ ProductService │  │  OrderService    │
│  port 5001   │  │   port 5002    │  │   port 5003      │
│ CWE-798 ⚠️  │  │CVE-2024-21907⚠️│  │CVE-2022-34716 ⚠️│
└──────────────┘  └────────────────┘  └────────┬─────────┘
                                                │ calls
                                                ▼
                                  ┌─────────────────────┐
                                  │ NotificationService  │
                                  │      port 5004       │
                                  │    CWE-319 ⚠️       │
                                  └─────────────────────┘
```

### Injected Vulnerabilities (Controlled Experiment)

| Service | CVE/CWE | Description | Severity |
|---------|---------|-------------|----------|
| OrderService | CVE-2022-34716 | .NET 6.0 runtime base image | CRITICAL |
| ProductService | CVE-2024-21907 | Newtonsoft.Json 12.0.1 | HIGH |
| IdentityService | CWE-798 | Hardcoded JWT secret | HIGH |
| NotificationService | CWE-319 | No HTTPS enforcement | MEDIUM |

> ⚠️ **Research Platform Only** — vulnerabilities are deliberately injected for controlled scanning experiments. Do not use in production.

---

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- Docker Desktop
- GitHub account (for Actions pipelines)
- Azure subscription (for Azure experiments)

### Run Locally

```bash
# Clone the repository
git clone https://github.com/suyashdworkspace/ShiftShield.git
cd ShiftShield

# Start all 5 services
docker-compose up --build

# Verify services are running
curl http://localhost:5000/health       # API Gateway
curl http://localhost:5001/api/auth     # Identity Service
curl http://localhost:5002/api/products # Product Service
curl http://localhost:5003/api/orders   # Order Service
curl http://localhost:5004/api/notify   # Notification Service
```

### Run Trivy Scan Manually

```bash
# Install Trivy
curl -sfL https://raw.githubusercontent.com/aquasecurity/trivy/main/contrib/install.sh | sh

# Scan a service image
trivy image --severity CRITICAL,HIGH order-service:latest
```

---

## ⚙️ CI/CD Pipeline Configurations

### Pipeline A — No Scan (Baseline)
```
Build → Push to Registry → Deploy
```
- No vulnerability scanning
- Establishes baseline CVE escape rate (100%) and build duration
- Trigger: `.github/workflows/pipeline-a.yml`

### Pipeline B — Shift-Left (Build-Stage Scanning)
```
Build → Trivy Scan → BLOCK if CRITICAL → Push to Registry
```
- Trivy scans all images **before** registry push
- Pipeline fails if CRITICAL CVEs detected
- Achieves 0% CVE escape rate
- Trigger: `.github/workflows/pipeline-b.yml`

### Pipeline C — Shift-Right (Post-Registry Scanning)
```
Build → Push to Registry → Trivy Scan → Alert
```
- All images pushed to registry **before** scanning
- Detects CVEs post-push — vulnerable images already in registry
- Registry escape rate: 100%
- Trigger: `.github/workflows/pipeline-c.yml`

---

## 🔬 Running the Experiment

### Step 1 — Configure GitHub Secrets

Add these secrets in GitHub → Settings → Secrets → Actions:

| Secret | Value |
|--------|-------|
| `AZURE_REGISTRY_URL` | `shiftshieldacr.azurecr.io` |
| `AZURE_REGISTRY_USERNAME` | ACR username |
| `AZURE_REGISTRY_PASSWORD` | ACR password |

### Step 2 — Trigger Pipelines (30 runs each)

Each pipeline uses a GitHub Actions matrix strategy to run 30 times automatically:

1. Go to **Actions** tab → select pipeline → **Run workflow**
2. One click = 30 sequential runs
3. Download CSV artifacts after completion

### Step 3 — Collect Metrics

```bash
# After downloading all artifacts to artifacts/ folder
python combine_metrics.py
# → Creates master_metrics.csv (180 rows)
```

### Step 4 — Statistical Analysis

```bash
python stats_analysis.py   # t-tests, ANOVA, descriptive stats
python graphs.py           # 4 matplotlib charts at 300 DPI
```

---

## 📁 Project Structure

```
ShiftShield/
├── .github/
│   └── workflows/
│       ├── pipeline-a.yml          # No scan baseline (Local)
│       ├── pipeline-b.yml          # Shift-left Trivy scan (Local)
│       ├── pipeline-c.yml          # Shift-right scan (Local)
│       ├── pipeline-a-azure.yml    # No scan baseline (Azure)
│       ├── pipeline-b-azure.yml    # Shift-left Trivy scan (Azure)
│       └── pipeline-c-azure.yml    # Shift-right scan (Azure)
├── IdentityService/
│   ├── Program.cs                  # Contains CWE-798 (hardcoded JWT)
│   └── Dockerfile
├── ProductService/
│   ├── ProductService.csproj       # Contains CVE-2024-21907
│   └── Dockerfile
├── OrderService/
│   ├── Dockerfile                  # Uses .NET 6.0 base (CVE-2022-34716)
│   └── ...
├── NotificationService/
│   ├── Program.cs                  # CWE-319 (no HTTPS)
│   └── Dockerfile
├── ApiGateway/
│   ├── ocelot.json
│   └── Dockerfile
├── docker-compose.yml
├── combine_metrics.py              # Merge 180 CSV artifacts
├── stats_analysis.py               # Statistical analysis
├── graphs.py                       # Generate 4 charts
└── artifacts/                      # Downloaded GitHub Actions artifacts
    └── metrics/
        ├── pipeline_a_run_1.csv
        └── ...
```

---

## 📊 Results Summary

### Pipeline Duration (Combined Local + Azure)
| Pipeline | Mean Duration | Overhead vs A |
|----------|--------------|---------------|
| A — No Scan | 64.60s | Baseline |
| B — Shift-Left | 75.77s | +17.3% |
| C — Shift-Right | 82.15s | +27.2% |

### Security Findings
- **Pipeline B** blocked 100% of CVEs in every run (0% escape rate)
- **Trivy** detected 6 CVEs per run consistently across all 120 scanning runs
- **MTTR** improved 80% with shift-left: 15 min vs 75 min post-deployment

### Statistical Significance
All key comparisons confirmed p < 0.05 via paired t-tests (critical value t > 2.045, df=29).

---

## 🛠️ Tech Stack

| Layer | Technology |
|-------|-----------|
| Services | .NET 8.0 Web API |
| Gateway | Ocelot Reverse Proxy |
| Containers | Docker, Docker Compose |
| CI/CD | GitHub Actions |
| Security Scanner | Trivy v0.48+ |
| Cloud Registry | Azure Container Registry |
| Cloud Runtime | Azure Container Apps |
| Statistics | Python scipy |
| Visualisation | Python matplotlib |

---

## 📄 License

MIT License — see [LICENSE](LICENSE) for details.

---

## 🎓 Academic Context

This repository is the experimental platform for a research paper submitted to:
- **Sanshodhan 2026** — International Student Research Conference, SIES College of Management Studies
- **MCARP42** — Research Paper course, MCA Semester IV, University of Mumbai

---

*SIES College of Management Studies, Navi Mumbai | Department of Computer Applications | 2025-2026*
