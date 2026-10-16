(()=>{
  const running=new WeakMap(),cleanups=new Set();
  const reduced=()=>matchMedia('(prefers-reduced-motion: reduce)').matches;
  const execute={play(button){
    if(!button)return;running.get(button)?.();
    const style=getComputedStyle(button),base={backgroundColor:style.backgroundColor,borderColor:style.borderTopColor,color:style.color};
    const green={backgroundColor:'#126a40',borderColor:'#42c785',color:'#f4fff7'};
    const duration=reduced()?150:700;
    const frames=reduced()?[green,green]:[{...green,offset:0},{...green,offset:.15},{...base,offset:1}];
    const animation=button.animate(frames,{duration,easing:'ease-out',fill:'none'});
    let ended=false,timer;
    const cleanup=()=>{if(ended)return;ended=true;clearTimeout(timer);animation.cancel();running.delete(button);cleanups.delete(cleanup);};
    running.set(button,cleanup);cleanups.add(cleanup);timer=setTimeout(cleanup,duration);
  }};
  const buttons=new Set();let state='idle',connected=false,failureReason='';
  const attachSync=()=>{for(const button of buttons){if(!button.isConnected){buttons.delete(button);continue;}button.dataset.sxAttach=state;const label=state==='success'?'Attached successfully':state==='failure'?'Attach failed: '+failureReason+'. Click to retry':state==='busy'?'Attaching, waiting for client confirmation':'Attach';button.setAttribute('aria-label',label);button.setAttribute('title',label);}};
  const attach={
    mount(button){if(!button)return;Promise.resolve().then(()=>{buttons.add(button);attachSync();});},
    begin(button){if(button)buttons.add(button);state='busy';failureReason='';window.synapseAttachErrors?.dismiss();attachSync();},
    finish(success,reason){state=success?'success':'failure';failureReason=success?'':(reason||'No detailed reason was returned by the runtime.');if(success)window.synapseAttachErrors?.dismiss();else window.synapseAttachErrors?.show(failureReason);attachSync();},
    connection(value){connected=!!value;if(state==='busy')return;if(connected){if(state!=='success')window.synapseAttachErrors?.dismiss();state='success';failureReason='';}else if(state==='success')state='idle';attachSync();}
  };
  window.synapseProgress=value=>typeof value==='number'&&Number.isFinite(value)?Math.max(0,Math.min(100,value)):null;
  window.synapseInstallTabShortcuts=({isReady,newTab,closeTab,cycleTab})=>{
    const handler=event=>{
      if(!(event.ctrlKey||event.metaKey)||event.altKey||event.isComposing||!isReady())return;
      const key=event.key.toLowerCase();
      if(key==='tab'){
        event.preventDefault();event.stopImmediatePropagation();cycleTab(event.shiftKey?-1:1);return;
      }
      if(event.shiftKey||(key!=='t'&&key!=='w'))return;
      event.preventDefault();event.stopImmediatePropagation();if(!event.repeat)(key==='t'?newTab:closeTab)();
    };
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
