import { useState, useEffect } from 'react'
import Chat from './Chat'
import './App.css'

interface User {
  authenticated: boolean
  name?: string
  avatar?: string
}

function App() {
  const [user, setUser] = useState<User | null>(null)

  useEffect(() => {
    fetch('/auth/user')
      .then(res => res.json())
      .then(setUser)
      .catch(() => setUser({ authenticated: false }))
  }, [])

  const logout = async () => {
    await fetch('/auth/logout', { method: 'POST' })
    setUser({ authenticated: false })
  }

  if (!user) return null

  return (
    <div className="app-container">
      <header className="app-header">
        <h1 className="app-title">Copilot Chat</h1>
        <p className="app-subtitle">Powered by GitHub Copilot SDK &amp; .NET Aspire</p>
        {user.authenticated && (
          <div className="user-info">
            {user.avatar && <img src={user.avatar} alt="" className="user-avatar" />}
            <span className="user-name">{user.name}</span>
            <button className="logout-button" onClick={logout}>Sign out</button>
          </div>
        )}
      </header>

      <main className="main-content">
        {user.authenticated ? (
          <Chat />
        ) : (
          <div className="login-prompt">
            <h2>Sign in to chat</h2>
            <p>Sign in with your GitHub account to use Copilot Chat.</p>
            <a href="/auth/login" className="login-button">
              <img src="/github.svg" alt="" width="20" height="20" />
              Sign in with GitHub
            </a>
          </div>
        )}
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
