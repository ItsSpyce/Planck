import { useState } from 'react';
import { BrowserRouter, Route, Routes } from 'react-router-dom';
import reactLogo from './assets/react.svg';
import './App.css';

function App() {
  const [count, setCount] = useState(0);

  return (
    <div className="App">
      <BrowserRouter>
        <ul>
          <li>
            <a href="/">Home</a>
          </li>
          <li>
            <a href="/subroute">Sub route</a>
          </li>
        </ul>
        <Routes>
          <Route element={<h1>Root path</h1>} index />
          <Route element={<h2>Sub route</h2>} path="/subroute" />
        </Routes>
      </BrowserRouter>
    </div>
  );
}

export default App;
