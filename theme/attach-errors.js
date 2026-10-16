(()=>{
  const fallbackReason='No detailed reason was returned by the runtime.';
  window.synapseAttachReason=error=>{
    if(typeof error==='string'){
      const text=error.trim();if(!text)return fallbackReason;
      try{const value=JSON.parse(text);if(value&&typeof value==='object')return window.synapseAttachReason(value);}catch{}
      return text;
    }
    if(error&&typeof error==='object'){
      if(typeof error.message==='string'&&error.message.trim())return error.message.trim();
      if(error.kind==='channelMismatch')return 'The Roblox installation uses a different update channel than this runtime supports. Use the channel repair dialog to continue.';
      if(typeof error.error==='string'&&error.error.trim())return error.error.trim();
      try{const text=JSON.stringify(error);if(text&&text!=='{}')return text;}catch{}
    }
    return fallbackReason;
  };
  const endpoint='http://127.0.0.1:19347';let token='',queue=Promise.resolve(),dialog=null,generation=0;
  async function request(path,options={}){
    const controller=new AbortController(),timer=setTimeout(()=>controller.abort(),2000);
    try{return await fetch(endpoint+path,{...options,signal:controller.signal,credentials:'omit',cache:'no-store'});}finally{clearTimeout(timer);}
  }
  async function notify(payload){
    for(let attempt=0;attempt<2;attempt++){
      if(!token){const response=await request('/session');if(!response.ok)throw Error('Error window unavailable');const session=await response.json();if(session.host!=='synapse-x-error-host'||session.version!==1||typeof session.token!=='string')throw Error('Unexpected error window host');token=session.token;}
      const response=await request('/notify',{method:'POST',headers:{'Content-Type':'application/json','X-Synapse-Token':token},body:JSON.stringify(payload)});
      if(response.status===403){token='';continue;}if(!response.ok)throw Error('Error window unavailable');return;
    }
    throw Error('Error window session expired');
  }
  function closeFallback(){if(dialog){dialog.close();dialog.remove();dialog=null;}}
  function showFallback(reason){
    closeFallback();const previous=document.activeElement;dialog=document.createElement('dialog');dialog.className='sx-attach-error';dialog.setAttribute('aria-labelledby','sx-attach-error-title');
    const title=document.createElement('h2');title.id='sx-attach-error-title';title.textContent='Attach failed';
    const text=document.createElement('p');text.className='sx-error-reason';text.textContent=reason;
    const hint=document.createElement('p');hint.className='sx-error-hint';hint.textContent='To show errors above other apps, open the main Synapse X.exe launcher from the extracted release.';
    const close=document.createElement('button');close.textContent='Close';close.addEventListener('click',()=>dialog?.close());
    const current=dialog;current.append(title,text,hint,close);current.addEventListener('close',()=>{if(dialog===current)dialog=null;current.remove();if(previous?.isConnected)previous.focus();},{once:true});document.body.append(current);current.showModal();close.focus();
  }
  window.synapseAttachErrors={
    show(reason){const current=++generation;closeFallback();queue=queue.catch(()=>{}).then(async()=>{if(current!==generation)return;try{await notify({type:'error',message:reason});}catch{if(current===generation)showFallback(reason);}});return queue;},
    dismiss(){++generation;closeFallback();queue=queue.catch(()=>{}).then(async()=>{try{await notify({type:'dismiss'});}catch{}});return queue;}
  };
})();
