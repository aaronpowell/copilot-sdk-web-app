import Chat from './Chat'
import './App.css'

function App() {
  return (
    <div className="app-container">
      <header className="app-header">
        <h1 className="app-title">Copilot Chat</h1>
        <p className="app-subtitle">Powered by GitHub Copilot SDK &amp; .NET Aspire</p>
      </header>

      <main className="main-content">
        <Chat />
      </main>

      <footer className="app-footer">
        <nav aria-label="Footer navigation">
          <a href="https://aspire.dev" target="_blank" rel="noopener noreferrer">
            .NET Aspire<span className="visually-hidden"> (opens in new tab)</span>
          </a>
          <a 
            href="https://github.com/dotnet/aspire" 
            target="_blank" 
            rel="noopener noreferrer"
            className="github-link"
            aria-label="View Aspire on GitHub (opens in new tab)"
          >
            <img src="/github.svg" alt="" width="24" height="24" aria-hidden="true" />
            <span className="visually-hidden">GitHub</span>
          </a>
        </nav>
      </footer>
    </div>
  )
}

export default App
