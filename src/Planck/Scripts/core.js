var planck=function(){"use strict";function E(e,n,s){return new Promise(i=>{function r(t){t.data.operationId===n&&(typeof s>"u"||s(t.data.body))&&(i(t.data.body),console.debug("Received webmessage",n,t.data),window.chrome.webview.removeEventListener("message",r))}window.chrome.webview.addEventListener("message",r),window.chrome.webview.postMessage({...e,operationId:n}),console.debug("Posted webmessage",n,e)})}function O(e,n,s){let i=null;function r(t){t.data.operationId===n&&(typeof s>"u"||s(t.data.body))&&(i=t.data.body,console.debug("Received webmessage",n,t.data),window.chrome.webview.removeEventListener("message",r))}return window.chrome.webview.addEventListener("message",r),window.chrome.webview.postMessage({...e,operationId:n}),console.debug("Posted sync webmessage",n,e),i}function S(){let e=-1;return{current:e,next(){return e===Number.MAX_SAFE_INTEGER?e=0:e++,e}}}function W(e,n){return new Promise((s,i)=>{const r=setInterval(()=>{e()&&(clearTimeout(t),clearInterval(r),s(!0))},5),t=setTimeout(()=>{i("Wait timed out")},n||1e3)})}const w=S();async function o(e,n){const s=w.next();return await E({command:e,body:n},s)}function m(e,n){const s=w.next();return O({command:e,body:n},s)}const D=1024;class b{get remainingBytes(){return this._ledger.length-this._ledger.position}constructor(n){this._isClosed=!1,this._ledger=n,this._buffer=new Uint8Array(Math.min(n.length,D))}canRead(){return this.remainingBytes>0}[Symbol.asyncIterator](){}async readToEnd(){if(this._isClosed)throw"Cannot read from a closed stream";if(!this.canRead())return this._buffer;const n=await o("READ_STREAM",{id:this._ledger.id});return this._buffer=_(n),this._ledger.position+=this._buffer.length,this._buffer}async readNext(){if(this._isClosed)throw"Cannot read from a closed stream";if(!this.canRead())return null;const n=await o("READ_STREAM_CHUNK",{id:this._ledger.id});return this._buffer=_(n),this._ledger.position+=this._buffer.length,this._buffer}data(){return this._buffer}text(){return new TextDecoder().decode(this._buffer)}async close(){await o("CLOSE_STREAM",{id:this._ledger.id}),this._isClosed=!0}}function _(e){return Uint8Array.from(atob(e),n=>n.charCodeAt(0))}const c=Object.create(null),d=[];window.chrome.webview.addEventListener("message",e=>{if(e.data?.command==="MODULE_PROP_CHANGED"){const{Name:n,Value:s,Module:i}=e.data.body;console.debug("Received module prop updated",n,i);const r=c[i];typeof r<"u"&&r.updateValue(n,s)}});async function L(e){if(d.includes(e))try{return console.debug("Loading in process, waiting",e),await W(()=>!d.includes(e)),console.debug("Completed wait for",e),c[e]}catch(i){console.debug(i)}if(e in c)return c[e];d.push(e);const n=await o("LOAD_MODULE",{id:e}),s=await A(e,n);return d.splice(d.indexOf(e),1),c[e]=s,s}const T="fn:";var u;(function(e){e.string="string",e.boolean="boolean",e.number="number",e.object="object",e.stream="stream",e.array="array",e.void="void",e.fn_string="fn:string",e.fn_boolean="fn:boolean",e.fn_number="fn:number",e.fn_object="fn:object",e.fn_stream="fn:stream",e.fn_array="fn:array",e.fn_void="fn:void"})(u||(u={}));async function A(e,n){const s=Object.create({updateValue(r,t){return s[r]=t}}),i=n.reduce((r,t)=>({...r,[t.name]:t}),Object.create(null));return new Proxy(s,{get(r,t,f){if(typeof r[t]<"u")return r[t];if(typeof t=="symbol")return Reflect.get(r,t,f);const a=i[t];if(!(typeof a>"u")&&a.hasGetter){if(a.returnType.startsWith(T))return async function(...l){const g=await o("INVOKE_MODULE_METHOD",{id:e,method:t,args:l});switch(a.returnType){case u.fn_stream:return new b(g);default:return g}};{if(typeof r[t]<"u")return r[t];const l=m("GET_MODULE_PROP",{prop:t,id:e,args:{}});switch(a.returnType){case u.stream:return r[t]=new b(l);default:return r[t]=l}}}},set(r,t,f,a){if(typeof t=="string"&&typeof i[t]<"u"&&!m("SET_MODULE_PROP",{value:f}))throw`Failed to update property ${t}, check logs`;return r[t]=f},deleteProperty(r,t){return typeof t=="string"&&typeof i[t]<"u"&&i[t].returnType===u.stream?(r[t].close(),delete r[t]):!1},has(r,t){return Object.hasOwn(r,t)}})}function h(e){return document.title=e,o("SET_WINDOW_TITLE",{title:e})}function I(e,n){return o("SET_WINDOW_SIZE",{width:e,height:n})}function R(){return o("SET_WINDOW_STATE","normal")}function N(){return o("SET_WINDOW_STATE","minimized")}function P(e){return o("SET_WINDOW_STATE",e)}var C=Object.freeze({__proto__:null,hideWindow:N,setTitle:h,setWindowSize:I,setWindowState:P,showWindow:R});function M(){window.addEventListener("contextmenu",e=>e.preventDefault()),document.addEventListener("DOMContentLoaded",()=>{v();const e=document.querySelector("title");new MutationObserver(s=>{s.forEach(v)}).observe(e,{childList:!0})})}function v(){h(document.title)}M();const y=Object.create(C);return Object.defineProperties(y,{sendMessage:{value:o,enumerable:!1,writable:!1},import:{enumerable:!1,writable:!1,value:L}}),y}();
