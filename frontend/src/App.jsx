import { useEffect, useState } from "react";
import axios from "axios";
import "./App.css";

const API="http://localhost:5048";

export default function App(){
 const [user,setUser]=useState(null);
 const [u,setU]=useState(""); const [p,setP]=useState("");
 const [assets,setAssets]=useState([]); const [tickets,setTickets]=useState([]);
 const [assetName,setAssetName]=useState(""); const [issue,setIssue]=useState("");
 const [selectedAsset,setSelectedAsset]=useState("");
 const [chat,setChat]=useState(""); const [chatResult,setChatResult]=useState("");

 const login = async () => {
  const r = await axios.post(API + "/login", { username: u, password: p });

  axios.defaults.headers.common['Authorization'] = "Bearer " + r.data.token;

  setUser(r.data);
  load();
};

 const load=async()=>{ setAssets((await axios.get(API+"/assets")).data); setTickets((await axios.get(API+"/tickets")).data); };

 useEffect(()=>{ if(user) load(); },[user]);

 if(!user) return(
 <div className="app-background">
  <div className="login-container">
   <div className="login-card">
    <div className="logo-wrapper">
     <h1 className="app-title">LabTrack Lite</h1>
     <p className="app-subtitle">Laboratory Asset Management System</p>
    </div>
    <div className="login-form">
     <div className="input-group">
      <input 
       className="form-input" 
       placeholder="Username" 
       value={u}
       onChange={e=>setU(e.target.value)}
       onKeyPress={e=>e.key==='Enter'&&login()}
      />
     </div>
     <div className="input-group">
      <input 
       className="form-input" 
       type="password" 
       placeholder="Password" 
       value={p}
       onChange={e=>setP(e.target.value)}
       onKeyPress={e=>e.key==='Enter'&&login()}
      />
     </div>
     <button className="btn-primary" onClick={login}>Sign In</button>
    </div>
   </div>
  </div>
 </div>);

 return(
 <div className="app-background">
  <div className="main-container">
   <header className="app-header">
    <div>
     <h1 className="welcome-title">Welcome back, {user.role}!</h1>
     <p className="welcome-subtitle">Manage your laboratory assets and tickets</p>
    </div>
   </header>

   <div className="content-grid">
    <Section title="Add Asset" icon="📦">
     <div className="form-row">
      <input 
       className="form-input" 
       placeholder="Enter asset name" 
       value={assetName} 
       onChange={e=>setAssetName(e.target.value)}
      />
      <button 
       className="btn-primary btn-icon" 
       onClick={async()=>{
        await axios.post(API+"/assets",{name:assetName,category:"General",qrCode:"QR"+Date.now(),status:"Available"}); 
        setAssetName(""); 
        load();
       }}
       disabled={!assetName.trim()}
      >
       <span>Add Asset</span>
      </button>
     </div>
    </Section>

    <Section title="Create Ticket" icon="🎫">
     <div className="form-column">
      <input 
       className="form-input" 
       placeholder="Issue title" 
       value={issue} 
       onChange={e=>setIssue(e.target.value)}
      />
      <select 
       className="form-select" 
       onChange={e=>setSelectedAsset(e.target.value)}
       value={selectedAsset}
      >
       <option value="">Select Asset</option>
       {assets.map(a=><option key={a.id} value={a.id}>{a.name}</option>)}
      </select>
      <button 
       className="btn-secondary btn-icon" 
       onClick={async()=>{
        await axios.post(API+"/tickets",{title:issue,status:"Open",assetId:selectedAsset}); 
        setIssue(""); 
        setSelectedAsset("");
        load();
       }}
       disabled={!issue.trim() || !selectedAsset}
      >
       <span>Raise Ticket</span>
      </button>
     </div>
    </Section>

    <Section title="AI Assistant" icon="🤖" className="chat-section">
     <div className="chat-container">
      <div className="form-row">
       <input 
        className="form-input" 
        placeholder="Ask me anything..." 
        value={chat} 
        onChange={e=>setChat(e.target.value)}
        onKeyPress={e=>e.key==='Enter'&&document.querySelector('.chat-btn')?.click()}
       />
       <button 
        className="btn-primary chat-btn" 
        onClick={async()=>setChatResult(JSON.stringify((await axios.post(API+"/chat/query",{query:chat})).data))}
        disabled={!chat.trim()}
       >
        Ask
       </button>
      </div>
      {chatResult && (
       <div className="chat-result">
        <div className="chat-result-content">{chatResult}</div>
       </div>
      )}
     </div>
    </Section>
   </div>

   <Grid title="Assets" icon="📦">
    {assets.length > 0 ? (
     assets.map(a=><Card key={a.id} title={a.name} status={a.status} category={a.category}/>)
    ) : (
     <div className="empty-state">No assets found</div>
    )}
   </Grid>
   
   <Grid title="Tickets" icon="🎫">
    {tickets.length > 0 ? (
     tickets.map(t=><Card key={t.id} title={t.title} status={t.status}/>)
    ) : (
     <div className="empty-state">No tickets found</div>
    )}
   </Grid>
  </div>
 </div>);
}

/* UI Components */
const Section=({title,icon,children,className})=>(
 <div className={`section-card ${className || ''}`}>
  <div className="section-header">
   <span className="section-icon">{icon}</span>
   <h3 className="section-title">{title}</h3>
  </div>
  <div className="section-content">{children}</div>
 </div>
);

const Grid=({title,icon,children})=>(
 <div className="grid-container">
  <div className="grid-header">
   <span className="grid-icon">{icon}</span>
   <h2 className="grid-title">{title}</h2>
  </div>
  <div className="card-grid">{children}</div>
 </div>
);

const Card=({title,status,category})=>{
 const statusClass = status?.toLowerCase().replace(/\s+/g, '-');
 return (
  <div className="asset-card">
   <div className="card-content">
    <h4 className="card-title">{title}</h4>
    {category && <span className="card-category">{category}</span>}
   </div>
   <div className={`card-status status-${statusClass}`}>
    <span className="status-dot"></span>
    {status}
   </div>
  </div>
 );
};
