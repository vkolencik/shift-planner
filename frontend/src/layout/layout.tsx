import React, { PropsWithChildren } from 'react'

export const FixedPage = ({children}: PropsWithChildren) => <div className='d-flex h-100 flex-column overflow-hidden'>{children}</div>
export const ScrollablePart = ({children}: PropsWithChildren) => <div className='flex-grow-1 overflow-auto'>{children}</div>
