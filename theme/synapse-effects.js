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
    const letters=[...word.children].map(span=>({text:span.textContent,rect:span.getBoundingClientRect()}));
    const animations=[],timers=[];let ended=false;
    const cleanup=()=>{if(ended)return;ended=true;timers.forEach(clearTimeout);animations.forEach(a=>a.cancel());overlay.remove();button.classList.remove('sx-executing');running.delete(button);cleanups.delete(cleanup);};
    running.set(button,cleanup);cleanups.add(cleanup);button.classList.add('sx-executing');
    const animate=(el,frames,options)=>{const a=el.animate(frames,options);animations.push(a);return a;};
    animate(word,[{transform:'translateX(0) skewX(0)',opacity:1},{transform:'translateX(6px) skewX(-9deg)',opacity:1,offset:.45},{transform:'translateX(18px) skewX(0)',opacity:1}],{duration:220,easing:'cubic-bezier(.22,.7,.25,1)',fill:'forwards'});
    const trail=document.createElement('i');trail.className='sx-execute-trail';trail.style.left=(rect.left+rect.width/2-28)+'px';trail.style.top=(rect.top+rect.height/2)+'px';overlay.append(trail);
    animate(trail,[{transform:'translateX(-8px) scaleX(.2)',opacity:0},{transform:'translateX(10px) scaleX(1)',opacity:.7,offset:.5},{transform:'translateX(35px) scaleX(.1)',opacity:0}],{duration:280,fill:'forwards'});
    timers.push(setTimeout(()=>{
      if(ended)return;word.style.visibility='hidden';
      for(let index=0;index<letters.length;index++){
        const letter=letters[index];if(!letter.text.trim())continue;
        for(let part=0;part<2;part++){
          const shard=document.createElement('span');shard.className='sx-execute-shard';shard.textContent=letter.text;
          Object.assign(shard.style,{font:style.font,color:style.color,left:(letter.rect.left+18)+'px',top:letter.rect.top+'px',width:letter.rect.width+'px',height:letter.rect.height+'px',clipPath:part?'polygon(100% 0,100% 100%,0 100%)':'polygon(0 0,100% 0,0 100%)'});overlay.append(shard);
          const direction=part?1:-1,vx=direction*(9+index*2),spin=direction*(25+index*6),frames=[];
          for(let step=0;step<=6;step++){const t=step/6;frames.push({offset:t,transform:`translate(${vx*t}px,${-5*t+90*t*t}px) rotate(${spin*t}deg)`,opacity:t<.45?1:Math.max(0,1-(t-.45)/.55)});}
          animate(shard,frames,{duration:650+index*24,easing:'linear',fill:'forwards'});
        }
      }
    },220));
    timers.push(setTimeout(cleanup,1250));
  }};
  const buttons=new Set();let state='idle',connected=false;
  const attachSync=()=>{for(const button of buttons){if(!button.isConnected){buttons.delete(button);continue;}button.dataset.sxAttach=state;const label=state==='success'?'Attached successfully':state==='failure'?'Attach failed. Click to retry':state==='busy'?'Attaching, waiting for client confirmation':'Attach';button.setAttribute('aria-label',label);button.setAttribute('title',label);}};
  const attach={
    mount(button){if(!button)return;Promise.resolve().then(()=>{buttons.add(button);attachSync();});},
    begin(button){if(button)buttons.add(button);state='busy';attachSync();},
    finish(success){state=success?'success':'failure';attachSync();},
    connection(value){connected=!!value;if(state==='busy')return;if(connected)state='success';else if(state==='success')state='idle';attachSync();}
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
