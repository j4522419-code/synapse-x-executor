(()=>{
  const running=new WeakMap(),cleanups=new Set();
  const reduced=()=>matchMedia('(prefers-reduced-motion: reduce)').matches;
  const execute={play(button){
    if(!button)return;running.get(button)?.();if(reduced())return;
    const rect=button.getBoundingClientRect(),style=getComputedStyle(button),text=button.textContent.trim();
    if(!text)return;
    const overlay=document.createElement('div');overlay.className='sx-execute-overlay';overlay.setAttribute('aria-hidden','true');
    const word=document.createElement('span');word.className='sx-execute-word';word.style.font=style.font;word.style.color=style.color;word.style.letterSpacing=style.letterSpacing;
    for(const char of text){const span=document.createElement('span');span.textContent=char===' '?'\u00a0':char;word.append(span);}
    overlay.append(word);document.body.append(overlay);
    const measure=word.getBoundingClientRect();word.style.left=(rect.left+(rect.width-measure.width)/2)+'px';word.style.top=(rect.top+(rect.height-measure.height)/2)+'px';
    const animations=[],timers=[];let ended=false;
    const cleanup=()=>{if(ended)return;ended=true;timers.forEach(clearTimeout);animations.forEach(a=>a.cancel());overlay.remove();button.classList.remove('sx-executing');running.delete(button);cleanups.delete(cleanup);};
    running.set(button,cleanup);cleanups.add(cleanup);button.classList.add('sx-executing');
    // Fifty spatial changes in 500 ms; no brightness/opacity flashing.
    const positions=Array.from({length:51},(_,i)=>i===0||i===50?{x:0,y:0}:{x:Math.sin(i*2.399)*4,y:Math.cos(i*1.731)*2});
    const frames=positions.map((p,i)=>({offset:i/50,transform:`translate(${p.x}px,${p.y}px)`}));
    animations.push(word.animate(frames,{duration:500,easing:'steps(1,end)',fill:'forwards'}));
    for(let lag=1;lag<=3;lag++){
      const trail=document.createElement('span');trail.className='sx-execute-ghost';trail.textContent=text;
      Object.assign(trail.style,{font:style.font,color:style.color,letterSpacing:style.letterSpacing,left:word.style.left,top:word.style.top,opacity:String(.18/lag)});
      overlay.prepend(trail);
      animations.push(trail.animate(positions.map((_,i)=>{const p=positions[Math.max(0,i-lag)];return{offset:i/50,transform:`translate(${p.x-lag*1.5}px,${p.y}px)`}}),{duration:500,easing:'steps(1,end)',fill:'forwards'}));
    }
    timers.push(setTimeout(cleanup,500));
  }};
  const buttons=new Set();let state='idle',connected=false;
  const attachSync=()=>{for(const button of buttons){if(!button.isConnected){buttons.delete(button);continue;}button.dataset.sxAttach=state;const label=state==='success'?'Attached successfully':state==='failure'?'Attach failed. Click to retry':state==='busy'?'Attaching, waiting for client confirmation':'Attach';button.setAttribute('aria-label',label);button.setAttribute('title',label);}};
  const attach={
    mount(button){if(!button)return;Promise.resolve().then(()=>{buttons.add(button);attachSync();});},
    begin(button){if(button)buttons.add(button);state='busy';attachSync();},
    finish(success){state=success?'success':'failure';attachSync();},
    connection(value){connected=!!value;if(state==='busy')return;if(connected)state='success';else if(state==='success')state='idle';attachSync();}
  };
  window.synapseProgress=value=>typeof value==='number'&&Number.isFinite(value)?Math.max(0,Math.min(100,value)):null;
  window.synapseInstallTabShortcuts=({isReady,newTab,closeTab})=>{
    const handler=event=>{if(!(event.ctrlKey||event.metaKey)||event.altKey||event.shiftKey||event.isComposing||!isReady())return;const key=event.key.toLowerCase();if(key!=='t'&&key!=='w')return;event.preventDefault();event.stopImmediatePropagation();if(!event.repeat)(key==='t'?newTab:closeTab)();};
    addEventListener('keydown',handler,{capture:true});return()=>removeEventListener('keydown',handler,{capture:true});
  };
  window.synapseFX={execute,attach};
  window.synapseConfirmAttach=async(invoke,isConnected,{now=()=>performance.now(),wait=ms=>new Promise(resolve=>setTimeout(resolve,ms)),timeout=30000}={})=>{
    await invoke();const deadline=now()+timeout;
    while(!isConnected()){if(now()>=deadline)throw Error('Attach failed: no client confirmation received.');await wait(100);}
  };
  window.synapseBrandNotes=input=>{
    const urls=[];let text=String(input??'').replace(/https?:\/\/[^\s<>\)]+/g,url=>{urls.push(url);return '\u0001SXURL'+(urls.length-1)+'\u0001';});
    text=text.replace(/potassium|grandassium/gi,'Synapse X');
    return text.replace(/\u0001SXURL(\d+)\u0001/g,(_,index)=>urls[Number(index)]);
  };
  document.addEventListener('visibilitychange',()=>{if(document.hidden)for(const cleanup of [...cleanups])cleanup();});
  const native=window.__TAURI_INTERNALS__;let titleBusy=false;
  async function syncTitle(){
    document.title='Synapse X';if(!native||titleBusy)return;titleBusy=true;
    try{const label=native.metadata?.currentWindow?.label||'main';const current=await native.invoke('plugin:window|title',{label});if(current!=='Synapse X')await native.invoke('plugin:window|set_title',{label,value:'Synapse X'});}catch{}finally{titleBusy=false;}
  }
  void syncTitle();if(native){const timer=setInterval(syncTitle,2000);addEventListener('focus',syncTitle);addEventListener('pagehide',()=>clearInterval(timer),{once:true});}
})();
