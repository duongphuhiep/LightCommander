export default function Home() {
  const messages = [
    { role: 'user', content: 'What is the capital of France?' },
    {
      role: 'assistant',
      thinking:
        'The user is asking about the capital of France. This is a simple geography question.',
      content: 'The capital of France is **Paris**.',
    },
    { role: 'user', content: 'Tell me more about it.' },
  ];

  return (
    <div className='flex flex-col h-screen bg-base-200'>
      {/* Chat Panel */}
      <div className='flex-1 overflow-y-auto p-4 space-y-4'>
        {messages.map((msg, i) => (
          <div
            key={"k"+i}
            className={`chat ${
              msg.role === 'user' ? 'chat-end' : 'chat-start'
            }`}>
            <div className='chat-header opacity-50 text-xs mb-1'>
              {msg.role === 'user' ? 'You' : 'AI'}
            </div>

            {/* Thinking steps */}
            {msg.thinking && (
              <div className='chat-bubble chat-bubble-warning opacity-70 text-sm italic mb-1'>
                💭 {msg.thinking}
              </div>
            )}

            <div
              className={`chat-bubble ${
                msg.role === 'user' ? 'chat-bubble-primary' : ''
              }`}>
              {msg.content}
            </div>
          </div>
        ))}

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
