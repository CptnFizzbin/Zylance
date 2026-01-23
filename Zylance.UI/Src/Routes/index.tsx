import { createFileRoute } from '@tanstack/react-router';
import logo from '../logo.svg';
import { useState } from 'react';
import { useZylance } from '@/Lib/ZylanceContext';

export const Route = createFileRoute('/')({
  component: App,
});

function App () {
  const zylanceApi = useZylance();
  const [lastMessage, setLastMessage] = useState('');

  const onBtnClick = async () => {
    const res = await zylanceApi.EchoMessage({ message: 'Hello from Zylence!' });
    setLastMessage(res.echoed);
  };

  return (
    <div className="text-center">
      <header className="min-h-screen flex flex-col items-center justify-center bg-[#282c34] text-white text-[calc(10px+2vmin)]">
        <img
          src={logo}
          className="h-[40vmin] pointer-events-none animate-[spin_20s_linear_infinite]"
          alt="logo"
        />
        <p>
          Edit <code>src/routes/index.tsx</code> and save to reload
        </p>

        <button type="button" onClick={onBtnClick}>Send Message to Backend</button>
        {lastMessage && <p>Last message from backend: {lastMessage}</p>}

        <a
          className="text-[#61dafb] hover:underline"
          href="https://reactjs.org"
          target="_blank"
          rel="noopener noreferrer"
        >
          Learn React
        </a>
        <a
          className="text-[#61dafb] hover:underline"
          href="https://tanstack.com"
          target="_blank"
          rel="noopener noreferrer"
        >
          Learn TanStack
        </a>
      </header>
    </div>
  );
}
