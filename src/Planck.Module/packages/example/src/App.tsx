import { useEffect, useState } from 'react';
import reactLogo from './assets/react.svg';
import './App.css';
import { BrowserRouter, Link, Route, Routes } from 'react-router-dom';

function App() {
	const [count, setCount] = useState(0);

	useEffect(() => {
		window.chrome.webview.postMessage({
			command: 'SET_WINDOW_TITLE__request__0',
			body: { title: `Count is ${count}` },
		});
	}, [count]);

	return (
		<BrowserRouter>
			<div className="App">
				<Link to="/test">Test link</Link>
				<Routes>
					<Route
						index
						element={
							<>
								<div>
									<a href="https://vitejs.dev" target="_blank">
										<img src="/vite.svg" className="logo" alt="Vite logo" />
									</a>
									<a href="https://reactjs.org" target="_blank">
										<img
											src={reactLogo}
											className="logo react"
											alt="React logo"
										/>
									</a>
								</div>
								<h1>Vite + React</h1>
								<div className="card">
									<button onClick={() => setCount((count) => count + 1)}>
										count is {count}
									</button>
									<p>
										Edit <code>src/App.tsx</code> and save to test HMR
									</p>
								</div>
								<p className="read-the-docs">
									Click on the Vite and React logos to learn more
								</p>
							</>
						}
					/>
					<Route path="test" element={<h1>This is a test</h1>} />
				</Routes>
			</div>
		</BrowserRouter>
	);
}

export default App;
