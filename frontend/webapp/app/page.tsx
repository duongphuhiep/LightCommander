export default function Home() {
  return (
    <div className='flex flex-col h-screen bg-base-200'>
      {/* Chat Panel */}
      <div className='flex-1 overflow-y-auto p-4 space-y-4'>
        <div className='chat chat-end'>
          <div className='chat-bubble chat-bubble-primary'>
            What is the capital of France?
          </div>
        </div>

        <div className='chat chat-start'>
          <div className='chat-header opacity-50 text-xs mb-1'>
            {/* Thinking steps */}
            <div className='opacity-70 text-sm italic mb-1'>
              💭 The user is asking about the capital of France. This is a
              simple geography question.
            </div>
          </div>

          <div className='chat-bubble'>The capital of France is **Paris**.</div>
        </div>

        {/* AI streaming indicator */}
        <div className='chat chat-start'>
          <div className='chat-header opacity-50 text-xs mb-1'>AI</div>
          <div className='chat-bubble'>
            <span className='loading loading-dots loading-sm' />
          </div>
        </div>
      </div>

      {/* Input Panel */}
      <div className='p-4 bg-base-100 border-t border-base-300'>
        <div className='flex gap-2 max-w-3xl mx-auto'>
          <input
            type='text'
            placeholder='Type a message...'
            className='input input-bordered flex-1'
          />
          <button className='btn btn-error'>Stop</button>
        </div>
      </div>
    </div>
  );
}
